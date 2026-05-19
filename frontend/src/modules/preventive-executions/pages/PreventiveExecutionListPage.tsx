import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { StatusBadge } from '@/components/status/StatusBadge'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'
import { preventiveExecutionQueryKeys } from '@/modules/preventive-executions/api/preventive-execution-query-keys'
import type { PreventiveExecutionListFilters } from '@/modules/preventive-executions/types/preventive-execution'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'
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
  const { locale, t } = useTranslation()
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
        <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
      </section>
    )
  }

  if (executionsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
        <h1>{t('executions.loadingTitle')}</h1>
      </section>
    )
  }

  if (executionsQuery.isError) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
        <h1>{t('executions.errorTitle')}</h1>
        <p>{executionsQuery.error.message}</p>
      </section>
    )
  }

  const executions = executionsQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
          <h1>{t('executions.listTitle')}</h1>
        </div>
        <p>{t('executions.listDescription')}</p>
      </div>

      <div className="module-panel__actions">
        {canExecute ? (
          <Link className="button" to={routePaths.preventiveExecutionCreate}>
            {t('executions.start')}
          </Link>
        ) : null}
      </div>

      <section className="form-section">
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="executionStatusFilter">{t('common.status')}</label>
            <select
              id="executionStatusFilter"
              value={filters.status}
              onChange={(event) => setFilters({ ...filters, status: event.target.value })}
            >
              <option value="">{t('common.allStatuses')}</option>
              <option value="draft">{t('status.draft')}</option>
              <option value="submitted">{t('status.submitted')}</option>
              <option value="approved">{t('status.approved')}</option>
              <option value="rejected">{t('status.rejected')}</option>
              <option value="reworkRequested">{t('status.reworkRequested')}</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="executionSearchFilter">{t('common.search')}</label>
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
            {t('executions.currentUserOnly')}
          </label>
        </div>
      </section>

      {executions.length === 0 ? (
        <div className="empty-state">
          <h2>{t('executions.emptyTitle')}</h2>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">{t('executions.inventoryItem')}</th>
                <th scope="col">{t('common.entityType')}</th>
                <th scope="col">{t('executions.template')}</th>
                <th scope="col">{t('common.status')}</th>
                <th scope="col">{t('executions.startedBy')}</th>
                <th scope="col">{t('executions.lastUpdated')}</th>
                <th scope="col">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {executions.map((execution) => (
                <tr key={execution.id}>
                  <td>
                    <strong>{execution.inventoryItemDisplayName}</strong>
                    <small>{localizeDemoText(execution.siteName, locale)}</small>
                  </td>
                  <td>{localizeDemoText(execution.entityTypeName, locale)}</td>
                  <td>{localizeDemoText(execution.preventiveTemplateName, locale)}</td>
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
                        {t('common.view')}
                      </Link>
                      {canExecute && execution.status === 'draft' ? (
                        <Link
                          className="button--secondary"
                          to={buildPreventiveExecutionEditPath(execution.id)}
                        >
                          {t('executions.resume')}
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
