import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { StatusBadge } from '@/components/status/StatusBadge'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { preventiveValidationQueryKeys } from '@/modules/preventive-validations/api/preventive-validation-query-keys'
import { preventiveValidationsClient } from '@/modules/preventive-validations/api/preventive-validations-client'
import type { PreventiveValidationListFilters } from '@/modules/preventive-validations/types/preventive-validation'
import { buildPreventiveValidationDetailPath } from '@/shared/routing/route-paths'

const defaultFilters: PreventiveValidationListFilters = {
  status: 'submitted',
  entityTypeId: '',
  inventoryItemId: '',
  siteId: '',
  regionId: '',
  submittedBy: '',
  search: '',
}

export function PreventiveValidationQueuePage() {
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken
  const [filters, setFilters] = useState(defaultFilters)

  const validationsQuery = useQuery({
    queryKey: preventiveValidationQueryKeys.list(filters),
    queryFn: () => preventiveValidationsClient.list(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive validation</p>
        <h1>Authenticated access is required.</h1>
      </section>
    )
  }

  if (validationsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive validation</p>
        <h1>Loading validation queue.</h1>
      </section>
    )
  }

  if (validationsQuery.isError) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive validation</p>
        <h1>Validation queue could not be loaded.</h1>
        <p>{validationsQuery.error.message}</p>
      </section>
    )
  }

  const validations = validationsQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Preventive validation</p>
          <h1>Submitted executions awaiting review.</h1>
        </div>
        <p>Review execution snapshots, answers, comments, and validation history.</p>
      </div>

      <section className="form-section">
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="validationStatusFilter">Status</label>
            <select
              id="validationStatusFilter"
              value={filters.status}
              onChange={(event) => setFilters({ ...filters, status: event.target.value })}
            >
              <option value="">All statuses</option>
              <option value="submitted">Submitted</option>
              <option value="approved">Approved</option>
              <option value="rejected">Rejected</option>
              <option value="reworkRequested">Rework requested</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="validationSearchFilter">Search</label>
            <input
              id="validationSearchFilter"
              type="text"
              value={filters.search}
              onChange={(event) => setFilters({ ...filters, search: event.target.value })}
            />
          </div>
        </div>
      </section>

      {validations.length === 0 ? (
        <div className="empty-state">
          <h2>No preventive executions match the validation filters.</h2>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">Inventory item</th>
                <th scope="col">Entity type</th>
                <th scope="col">Template</th>
                <th scope="col">Submitted by</th>
                <th scope="col">Submitted at</th>
                <th scope="col">Site</th>
                <th scope="col">Region</th>
                <th scope="col">Status</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {validations.map((execution) => (
                <tr key={execution.id}>
                  <td>
                    <strong>{execution.inventoryItemDisplayName}</strong>
                  </td>
                  <td>{execution.entityTypeName}</td>
                  <td>{execution.preventiveTemplateName}</td>
                  <td>{execution.submittedBy ?? 'Not submitted'}</td>
                  <td>
                    {execution.submittedAtUtc
                      ? new Date(execution.submittedAtUtc).toLocaleString()
                      : 'Not submitted'}
                  </td>
                  <td>{execution.siteName}</td>
                  <td>{execution.regionName}</td>
                  <td>
                    <StatusBadge value={execution.status} />
                  </td>
                  <td>
                    <Link
                      className="button--secondary"
                      to={buildPreventiveValidationDetailPath(execution.id)}
                    >
                      Review
                    </Link>
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
