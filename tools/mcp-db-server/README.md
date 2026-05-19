# InfraOps MCP Database Server

`tools/mcp-db-server` is a local stdio MCP server for safe PostgreSQL schema inspection during InfraOps development.

## Exposed Tools

- `list_tables`
- `describe_table`
- `list_columns`
- `list_foreign_keys`
- `get_schema_summary`
- `run_readonly_query`

## Safety Model

This server is intentionally narrow:

- schema and table identifiers must be plain unquoted PostgreSQL identifiers
- `run_readonly_query` only accepts a single `SELECT`, `WITH`, or `EXPLAIN` statement
- SQL comments and multiple statements are rejected
- common DDL and DML keywords are blocked before execution
- read-only queries run inside a PostgreSQL `BEGIN READ ONLY` transaction
- query execution uses a configurable statement timeout

This server is for local development only. For stricter setups, point it at a dedicated read-only PostgreSQL role.

## Environment Variables

- `DATABASE_URL`
- `DB_SCHEMA`
- `MCP_DB_MAX_RESULT_ROWS`
- `MCP_DB_STATEMENT_TIMEOUT_MS`

If `DATABASE_URL` is not supplied, the standard PostgreSQL variables are also supported:

- `PGHOST`
- `PGPORT`
- `PGDATABASE`
- `PGUSER`
- `PGPASSWORD`

## Run Manually

```powershell
cd .\tools\mcp-db-server
npm install
$env:DATABASE_URL = "postgresql://infraops:infraops@localhost:5432/infraops"
$env:DB_SCHEMA = "public"
npm start
```

The server communicates over stdio, so it is meant to be started by an MCP client such as Codex.
