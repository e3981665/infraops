import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js'
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js'
import { Pool } from 'pg'
import * as z from 'zod/v4'

const DEFAULT_SCHEMA = process.env.DB_SCHEMA?.trim() || 'public'
const DEFAULT_MAX_RESULT_ROWS = clampInteger(
  process.env.MCP_DB_MAX_RESULT_ROWS,
  1,
  200,
  50,
)
const STATEMENT_TIMEOUT_MS = clampInteger(
  process.env.MCP_DB_STATEMENT_TIMEOUT_MS,
  1000,
  30000,
  5000,
)

let pool

const server = new McpServer({
  name: 'infraops-db-server',
  version: '0.1.0',
})

const schemaInput = z
  .string()
  .trim()
  .min(1)
  .optional()
  .describe('Database schema name. Defaults to DB_SCHEMA or public.')

const tableInput = z
  .string()
  .trim()
  .min(1)
  .describe('Unquoted table name inside the selected schema.')

server.registerTool(
  'list_tables',
  {
    description:
      'List base tables in the configured PostgreSQL schema.',
    inputSchema: {
      schema: schemaInput,
    },
  },
  async ({ schema }) =>
    executeTool(async () => {
      const resolvedSchema = resolveIdentifier(schema ?? DEFAULT_SCHEMA, 'Schema')
      const rows = await queryRows(
        `
          select table_name
          from information_schema.tables
          where table_schema = $1
            and table_type = 'BASE TABLE'
          order by table_name;
        `,
        [resolvedSchema],
      )

      return {
        schema: resolvedSchema,
        tableCount: rows.length,
        tables: rows.map((row) => row.table_name),
      }
    }),
)

server.registerTool(
  'list_columns',
  {
    description:
      'List columns for a single table, including data type and nullability.',
    inputSchema: {
      table: tableInput,
      schema: schemaInput,
    },
  },
  async ({ schema, table }) =>
    executeTool(async () => {
      const resolvedSchema = resolveIdentifier(schema ?? DEFAULT_SCHEMA, 'Schema')
      const resolvedTable = resolveIdentifier(table, 'Table')
      const columns = await getColumns(resolvedSchema, resolvedTable)

      return {
        schema: resolvedSchema,
        table: resolvedTable,
        columns,
      }
    }),
)

server.registerTool(
  'list_foreign_keys',
  {
    description:
      'List foreign keys in the selected schema or for one table.',
    inputSchema: {
      schema: schemaInput,
      table: tableInput.optional(),
    },
  },
  async ({ schema, table }) =>
    executeTool(async () => {
      const resolvedSchema = resolveIdentifier(schema ?? DEFAULT_SCHEMA, 'Schema')
      const resolvedTable = table
        ? resolveIdentifier(table, 'Table')
        : undefined
      const foreignKeys = await getForeignKeys(resolvedSchema, resolvedTable)

      return {
        schema: resolvedSchema,
        table: resolvedTable ?? null,
        foreignKeys,
      }
    }),
)

server.registerTool(
  'describe_table',
  {
    description:
      'Describe one table with its columns, primary key, unique constraints, and foreign keys.',
    inputSchema: {
      table: tableInput,
      schema: schemaInput,
    },
  },
  async ({ schema, table }) =>
    executeTool(async () => {
      const resolvedSchema = resolveIdentifier(schema ?? DEFAULT_SCHEMA, 'Schema')
      const resolvedTable = resolveIdentifier(table, 'Table')

      const [columns, primaryKeyRows, uniqueConstraintRows, foreignKeys] =
        await Promise.all([
          getColumns(resolvedSchema, resolvedTable),
          queryRows(
            `
              select kcu.column_name
              from information_schema.table_constraints tc
              join information_schema.key_column_usage kcu
                on tc.constraint_name = kcu.constraint_name
               and tc.table_schema = kcu.table_schema
               and tc.table_name = kcu.table_name
              where tc.table_schema = $1
                and tc.table_name = $2
                and tc.constraint_type = 'PRIMARY KEY'
              order by kcu.ordinal_position;
            `,
            [resolvedSchema, resolvedTable],
          ),
          queryRows(
            `
              select
                tc.constraint_name,
                array_agg(kcu.column_name order by kcu.ordinal_position) as columns
              from information_schema.table_constraints tc
              join information_schema.key_column_usage kcu
                on tc.constraint_name = kcu.constraint_name
               and tc.table_schema = kcu.table_schema
               and tc.table_name = kcu.table_name
              where tc.table_schema = $1
                and tc.table_name = $2
                and tc.constraint_type = 'UNIQUE'
              group by tc.constraint_name
              order by tc.constraint_name;
            `,
            [resolvedSchema, resolvedTable],
          ),
          getForeignKeys(resolvedSchema, resolvedTable),
        ])

      if (columns.length === 0) {
        throw new Error(`Table '${resolvedSchema}.${resolvedTable}' was not found.`)
      }

      return {
        schema: resolvedSchema,
        table: resolvedTable,
        columns,
        primaryKey: primaryKeyRows.map((row) => row.column_name),
        uniqueConstraints: uniqueConstraintRows.map((row) => ({
          constraintName: row.constraint_name,
          columns: row.columns,
        })),
        foreignKeys,
      }
    }),
)

server.registerTool(
  'get_schema_summary',
  {
    description:
      'Summarize the selected schema with table counts, column counts, and foreign key counts.',
    inputSchema: {
      schema: schemaInput,
    },
  },
  async ({ schema }) =>
    executeTool(async () => {
      const resolvedSchema = resolveIdentifier(schema ?? DEFAULT_SCHEMA, 'Schema')

      const [tables, foreignKeyCountRows] = await Promise.all([
        queryRows(
          `
            select
              t.table_name,
              count(c.column_name)::int as column_count
            from information_schema.tables t
            left join information_schema.columns c
              on c.table_schema = t.table_schema
             and c.table_name = t.table_name
            where t.table_schema = $1
              and t.table_type = 'BASE TABLE'
            group by t.table_name
            order by t.table_name;
          `,
          [resolvedSchema],
        ),
        queryRows(
          `
            select count(*)::int as foreign_key_count
            from information_schema.table_constraints
            where table_schema = $1
              and constraint_type = 'FOREIGN KEY';
          `,
          [resolvedSchema],
        ),
      ])

      return {
        schema: resolvedSchema,
        tableCount: tables.length,
        foreignKeyCount: foreignKeyCountRows[0]?.foreign_key_count ?? 0,
        tables: tables.map((tableRow) => ({
          tableName: tableRow.table_name,
          columnCount: tableRow.column_count,
        })),
      }
    }),
)

server.registerTool(
  'run_readonly_query',
  {
    description:
      'Run one validated read-only SQL query against PostgreSQL. Only SELECT, WITH, and EXPLAIN statements are allowed.',
    inputSchema: {
      query: z
        .string()
        .trim()
        .min(1)
        .describe('A single read-only SQL statement.'),
      maxRows: z
        .number()
        .int()
        .min(1)
        .max(200)
        .optional()
        .describe('Optional maximum number of rows to return. Defaults to MCP_DB_MAX_RESULT_ROWS or 50.'),
    },
  },
  async ({ maxRows, query }) =>
    executeTool(async () => {
      const normalizedQuery = validateReadonlyQuery(query)
      const rowLimit = maxRows ?? DEFAULT_MAX_RESULT_ROWS
      const result = await runReadonlyQuery(normalizedQuery, rowLimit)

      return {
        rowCount: result.rowCount,
        rows: result.rows,
      }
    }),
)

async function executeTool(operation) {
  try {
    const payload = await operation()

    return {
      content: [
        {
          type: 'text',
          text: JSON.stringify(payload, null, 2),
        },
      ],
    }
  } catch (error) {
    return {
      isError: true,
      content: [
        {
          type: 'text',
          text: formatError(error),
        },
      ],
    }
  }
}

function clampInteger(rawValue, minimum, maximum, fallbackValue) {
  const parsedValue = Number.parseInt(rawValue ?? '', 10)

  if (!Number.isFinite(parsedValue)) {
    return fallbackValue
  }

  return Math.min(maximum, Math.max(minimum, parsedValue))
}

function getPool() {
  if (!pool) {
    assertConnectionConfiguration()

    pool = new Pool({
      connectionString: process.env.DATABASE_URL,
      host: process.env.PGHOST,
      port: process.env.PGPORT ? Number(process.env.PGPORT) : undefined,
      database: process.env.PGDATABASE,
      user: process.env.PGUSER,
      password: process.env.PGPASSWORD,
      application_name: 'infraops-mcp-db-server',
      max: 4,
    })
  }

  return pool
}

function assertConnectionConfiguration() {
  if (process.env.DATABASE_URL) {
    return
  }

  const requiredVariables = ['PGHOST', 'PGDATABASE', 'PGUSER']
  const missingVariables = requiredVariables.filter(
    (variableName) => !process.env[variableName],
  )

  if (missingVariables.length > 0) {
    throw new Error(
      `Database connection is not configured. Set DATABASE_URL or provide ${missingVariables.join(', ')}.`,
    )
  }
}

function resolveIdentifier(value, label) {
  const trimmedValue = value.trim()

  if (!/^[A-Za-z_][A-Za-z0-9_]*$/.test(trimmedValue)) {
    throw new Error(
      `${label} '${value}' is invalid. Use plain unquoted PostgreSQL identifiers only.`,
    )
  }

  return trimmedValue
}

async function queryRows(text, values) {
  const result = await getPool().query(text, values)

  return result.rows
}

async function getColumns(schema, table) {
  return queryRows(
    `
      select
        column_name,
        data_type,
        udt_name,
        is_nullable = 'YES' as is_nullable,
        column_default,
        ordinal_position
      from information_schema.columns
      where table_schema = $1
        and table_name = $2
      order by ordinal_position;
    `,
    [schema, table],
  )
}

async function getForeignKeys(schema, table) {
  const values = [schema]
  const tableFilter = table
    ? (() => {
        values.push(table)
        return 'and tc.table_name = $2'
      })()
    : ''

  return queryRows(
    `
      select
        tc.table_name,
        kcu.column_name,
        ccu.table_schema as referenced_table_schema,
        ccu.table_name as referenced_table_name,
        ccu.column_name as referenced_column_name,
        tc.constraint_name
      from information_schema.table_constraints tc
      join information_schema.key_column_usage kcu
        on tc.constraint_name = kcu.constraint_name
       and tc.table_schema = kcu.table_schema
      join information_schema.constraint_column_usage ccu
        on tc.constraint_name = ccu.constraint_name
       and tc.table_schema = ccu.table_schema
      where tc.constraint_type = 'FOREIGN KEY'
        and tc.table_schema = $1
        ${tableFilter}
      order by tc.table_name, tc.constraint_name, kcu.ordinal_position;
    `,
    values,
  )
}

function validateReadonlyQuery(query) {
  const trimmedQuery = query.trim()
  const withoutTrailingSemicolon = trimmedQuery.endsWith(';')
    ? trimmedQuery.slice(0, -1).trimEnd()
    : trimmedQuery
  const normalizedQuery = withoutTrailingSemicolon.toLowerCase()

  if (normalizedQuery.length === 0) {
    throw new Error('Query is required.')
  }

  if (withoutTrailingSemicolon.includes(';')) {
    throw new Error('Only a single SQL statement is allowed.')
  }

  if (/--|\/\*/.test(withoutTrailingSemicolon)) {
    throw new Error('SQL comments are not allowed in read-only queries.')
  }

  if (!/^(select|with|explain)\b/.test(normalizedQuery)) {
    throw new Error(
      'Only SELECT, WITH, and EXPLAIN statements are allowed in run_readonly_query.',
    )
  }

  const forbiddenPatterns = [
    /\binsert\b/,
    /\bupdate\b/,
    /\bdelete\b/,
    /\bdrop\b/,
    /\balter\b/,
    /\btruncate\b/,
    /\bcreate\b/,
    /\bgrant\b/,
    /\brevoke\b/,
    /\bcomment\b/,
    /\bcopy\b/,
    /\bcall\b/,
    /\bmerge\b/,
    /\bdo\b/,
    /\bvacuum\b/,
    /\breindex\b/,
    /\bcluster\b/,
    /\brefresh\b/,
    /\bbegin\b/,
    /\bcommit\b/,
    /\brollback\b/,
    /\bsavepoint\b/,
    /\block\b/,
    /\bsecurity\b/,
    /\bset\b/,
    /\breset\b/,
    /\blisten\b/,
    /\bnotify\b/,
    /\bunlisten\b/,
    /\bpg_sleep\b/,
    /\bfor\s+update\b/,
    /\binto\b/,
  ]

  if (forbiddenPatterns.some((pattern) => pattern.test(normalizedQuery))) {
    throw new Error(
      'The supplied SQL contains keywords that are not allowed in read-only mode.',
    )
  }

  return withoutTrailingSemicolon
}

async function runReadonlyQuery(query, maxRows) {
  const client = await getPool().connect()
  let transactionStarted = false

  try {
    await client.query('BEGIN READ ONLY')
    transactionStarted = true
    await client.query(`SET LOCAL statement_timeout = '${STATEMENT_TIMEOUT_MS}ms'`)

    const normalizedQuery = query.trim().toLowerCase()
    const result = normalizedQuery.startsWith('explain')
      ? await client.query(query)
      : await client.query(
          `select * from (${query}) as infraops_readonly_query limit $1`,
          [maxRows],
        )

    await client.query('COMMIT')

    return {
      rowCount: result.rowCount ?? result.rows.length,
      rows: result.rows,
    }
  } catch (error) {
    if (transactionStarted) {
      try {
        await client.query('ROLLBACK')
      } catch {
        // Ignore rollback failures and surface the original query error instead.
      }
    }

    throw error
  } finally {
    client.release()
  }
}

function formatError(error) {
  if (error instanceof Error) {
    return error.message
  }

  return 'Unexpected server error.'
}

async function closePool() {
  if (pool) {
    await pool.end()
    pool = undefined
  }
}

for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, async () => {
    await closePool()
    process.exit(0)
  })
}

async function main() {
  const transport = new StdioServerTransport()
  await server.connect(transport)
}

main().catch(async (error) => {
  console.error(formatError(error))
  await closePool()
  process.exit(1)
})
