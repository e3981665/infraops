import { useQuery } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router-dom'
import { StatusBadge } from '@/components/status/StatusBadge'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { preventiveValidationQueryKeys } from '@/modules/preventive-validations/api/preventive-validation-query-keys'
import { preventiveValidationsClient } from '@/modules/preventive-validations/api/preventive-validations-client'
import type { PreventiveValidationListFilters } from '@/modules/preventive-validations/types/preventive-validation'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'
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

const allowedStatusFilters = new Set(['', 'submitted', 'approved', 'rejected', 'reworkRequested'])

function normalizeStatusFilter(value: string | null) {
  if (value && allowedStatusFilters.has(value)) {
    return value
  }

  return defaultFilters.status
}

export function PreventiveValidationQueuePage() {
  const { session } = useAuthSession()
  const { locale, t } = useTranslation()
  const accessToken = session?.tokens.accessToken
  const [searchParams, setSearchParams] = useSearchParams()
  const filters: PreventiveValidationListFilters = {
    ...defaultFilters,
    status: normalizeStatusFilter(searchParams.get('status')),
    search: searchParams.get('search') ?? '',
  }

  function updateFilter(name: 'status' | 'search', value: string) {
    const nextSearchParams = new URLSearchParams(searchParams)

    if (!value || (name === 'status' && value === defaultFilters.status)) {
      nextSearchParams.delete(name)
    } else {
      nextSearchParams.set(name, value)
    }

    setSearchParams(nextSearchParams, { replace: true })
  }

  const validationsQuery = useQuery({
    queryKey: preventiveValidationQueryKeys.list(filters),
    queryFn: () => preventiveValidationsClient.list(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('validations.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
      </section>
    )
  }

  if (validationsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('validations.eyebrow')}</p>
        <h1>{t('validations.loadingTitle')}</h1>
      </section>
    )
  }

  if (validationsQuery.isError) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('validations.eyebrow')}</p>
        <h1>{t('validations.errorTitle')}</h1>
        <p>{validationsQuery.error.message}</p>
      </section>
    )
  }

  const validations = validationsQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('validations.eyebrow')}</p>
          <h1>{t('validations.queueTitle')}</h1>
        </div>
        <p>{t('validations.queueDescription')}</p>
      </div>

      <section className="form-section">
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="validationStatusFilter">{t('common.status')}</label>
            <select
              id="validationStatusFilter"
              value={filters.status}
              onChange={(event) => updateFilter('status', event.target.value)}
            >
              <option value="">{t('common.allStatuses')}</option>
              <option value="submitted">{t('status.submitted')}</option>
              <option value="approved">{t('status.approved')}</option>
              <option value="rejected">{t('status.rejected')}</option>
              <option value="reworkRequested">{t('status.reworkRequested')}</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="validationSearchFilter">{t('common.search')}</label>
            <input
              id="validationSearchFilter"
              type="text"
              value={filters.search}
              onChange={(event) => updateFilter('search', event.target.value)}
            />
          </div>
        </div>
      </section>

      {validations.length === 0 ? (
        <div className="empty-state">
          <h2>{t('validations.emptyTitle')}</h2>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">{t('executions.inventoryItem')}</th>
                <th scope="col">{t('common.entityType')}</th>
                <th scope="col">{t('executions.template')}</th>
                <th scope="col">{t('validations.submittedBy')}</th>
                <th scope="col">{t('validations.submittedAt')}</th>
                <th scope="col">{t('common.site')}</th>
                <th scope="col">{t('common.region')}</th>
                <th scope="col">{t('common.status')}</th>
                <th scope="col">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {validations.map((execution) => (
                <tr key={execution.id}>
                  <td>
                    <strong>{execution.inventoryItemDisplayName}</strong>
                  </td>
                  <td>{localizeDemoText(execution.entityTypeName, locale)}</td>
                  <td>{localizeDemoText(execution.preventiveTemplateName, locale)}</td>
                  <td>{execution.submittedBy ?? t('validations.notSubmitted')}</td>
                  <td>
                    {execution.submittedAtUtc
                      ? new Date(execution.submittedAtUtc).toLocaleString()
                      : t('validations.notSubmitted')}
                  </td>
                  <td>{localizeDemoText(execution.siteName, locale)}</td>
                  <td>{localizeDemoText(execution.regionName, locale)}</td>
                  <td>
                    <StatusBadge value={execution.status} />
                  </td>
                  <td>
                    <Link
                      className="button--secondary"
                      to={buildPreventiveValidationDetailPath(execution.id)}
                    >
                      {t('validations.review')}
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
