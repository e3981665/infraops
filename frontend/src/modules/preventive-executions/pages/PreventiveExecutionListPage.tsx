import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { StatusBadge } from '@/components/status/StatusBadge'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'
import { preventiveExecutionQueryKeys } from '@/modules/preventive-executions/api/preventive-execution-query-keys'
import type { PreventiveExecutionListFilters } from '@/modules/preventive-executions/types/preventive-execution'
import {
  buildPreventiveExecutionDetailPath,
  buildPreventiveExecutionEditPath,
  routePaths,
} from '@/shared/routing/route-paths'

const defaultFilters: PreventiveExecutionListFilters = {
  status: '',
  entityTypeId: '',
  inventoryItemId: '',
  siteId: '',
  regionId: '',
  createdByCurrentUser: false,
  search: '',
}

export function PreventiveExecutionListPage() {
  const { hasPermission, session } = useAuthSession()
  const accessToken = session?.tokens.accessToken
  const [filters, setFilters] = useState(defaultFilters)
  const canExecute = hasPermission(permissionCodes.preventiveExecute)

  const executionsQuery = useQuery({
    queryKey: preventiveExecutionQueryKeys.list(filters),
    queryFn: () => preventiveExecutionsClient.list(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive execution</p>
        <h1>Authenticated access is required.</h1>
      </section>
    )
  }

  if (executionsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive execution</p>
        <h1>Loading executions.</h1>
      </section>
    )
  }

  if (executionsQuery.isError) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive execution</p>
        <h1>Executions could not be loaded.</h1>
        <p>{executionsQuery.error.message}</p>
      </section>
    )
  }

  const executions = executionsQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Preventive execution</p>
          <h1>Maintenance checklist executions.</h1>
        </div>
        <p>Start, resume, and review operational preventive maintenance executions.</p>
      </div>

      <div className="module-panel__actions">
        {canExecute ? (
          <Link className="button" to={routePaths.preventiveExecutionCreate}>
            Start execution
          </Link>
        ) : null}
      </div>

      <section className="form-section">
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="executionStatusFilter">Status</label>
            <select
              id="executionStatusFilter"
              value={filters.status}
              onChange={(event) => setFilters({ ...filters, status: event.target.value })}
            >
              <option value="">All statuses</option>
              <option value="draft">Draft</option>
              <option value="submitted">Submitted</option>
              <option value="approved">Approved</option>
              <option value="rejected">Rejected</option>
              <option value="reworkRequested">Rework requested</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="executionSearchFilter">Search</label>
            <input
              id="executionSearchFilter"
              type="text"
              value={filters.search}
              onChange={(event) => setFilters({ ...filters, search: event.target.value })}
            />
          </div>
          <label className="checkbox-field" htmlFor="executionMineFilter">
            <input
              id="executionMineFilter"
              checked={filters.createdByCurrentUser}
              type="checkbox"
              onChange={(event) =>
                setFilters({ ...filters, createdByCurrentUser: event.target.checked })
              }
            />
            Current user only
          </label>
        </div>
      </section>

      {executions.length === 0 ? (
        <div className="empty-state">
          <h2>No preventive executions match the current filters.</h2>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">Inventory item</th>
                <th scope="col">Entity type</th>
                <th scope="col">Template</th>
                <th scope="col">Status</th>
                <th scope="col">Started by</th>
                <th scope="col">Last updated</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {executions.map((execution) => (
                <tr key={execution.id}>
                  <td>
                    <strong>{execution.inventoryItemDisplayName}</strong>
                    <small>{execution.siteName}</small>
                  </td>
                  <td>{execution.entityTypeName}</td>
                  <td>{execution.preventiveTemplateName}</td>
                  <td>
                    <StatusBadge value={execution.status} />
                  </td>
                  <td>{execution.createdBy}</td>
                  <td>{new Date(execution.updatedAtUtc).toLocaleString()}</td>
                  <td>
                    <div className="inline-actions">
                      <Link
                        className="button--secondary"
                        to={buildPreventiveExecutionDetailPath(execution.id)}
                      >
                        View
                      </Link>
                      {canExecute && execution.status === 'draft' ? (
                        <Link
                          className="button--secondary"
                          to={buildPreventiveExecutionEditPath(execution.id)}
                        >
                          Resume
                        </Link>
                      ) : null}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
