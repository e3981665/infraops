import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { dashboardClient } from '@/modules/dashboard/api/dashboard-client'
import { dashboardQueryKeys } from '@/modules/dashboard/api/dashboard-query-keys'
import { MetricCard } from '@/modules/dashboard/components/MetricCard'
import { SimpleBarChart } from '@/modules/dashboard/components/SimpleBarChart'
import type { DashboardFilters, DashboardMetricPoint } from '@/modules/dashboard/types/dashboard'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'
import { routePaths } from '@/shared/routing/route-paths'

const defaultFilters: DashboardFilters = {
  regionId: '',
  siteId: '',
  entityTypeId: '',
  fromUtc: '',
  toUtc: '',
}

export function DashboardPage() {
  const { session } = useAuthSession()
  const { locale, t } = useTranslation()
  const accessToken = session?.tokens.accessToken
  const [filters, setFilters] = useState(defaultFilters)

  const summaryQuery = useQuery({
    queryKey: dashboardQueryKeys.summary(filters),
    queryFn: () => dashboardClient.getSummary(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  const chartsQuery = useQuery({
    queryKey: dashboardQueryKeys.charts(filters),
    queryFn: () => dashboardClient.getCharts(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('dashboard.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
      </section>
    )
  }

  if (summaryQuery.isLoading || chartsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('dashboard.eyebrow')}</p>
        <h1>{t('dashboard.loadingTitle')}</h1>
        <p>{t('dashboard.loadingMessage')}</p>
      </section>
    )
  }

  if (summaryQuery.isError || chartsQuery.isError || !summaryQuery.data || !chartsQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('dashboard.eyebrow')}</p>
        <h1>{t('dashboard.errorTitle')}</h1>
        <p>{summaryQuery.error?.message ?? chartsQuery.error?.message}</p>
      </section>
    )
  }

  const summary = summaryQuery.data
  const charts = chartsQuery.data
  const localizePoints = (points: DashboardMetricPoint[]) =>
    points.map((point) => ({
      ...point,
      label: localizeDemoText(point.label, locale),
    }))

  return (
    <section className="module-panel dashboard-workspace">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('dashboard.eyebrow')}</p>
          <h1>{t('dashboard.title')}</h1>
        </div>
        <p>{t('dashboard.description')}</p>
      </div>

      <section className="form-section">
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="dashboardFrom">{t('common.from')}</label>
            <input
              id="dashboardFrom"
              type="datetime-local"
              value={filters.fromUtc}
              onChange={(event) => setFilters({ ...filters, fromUtc: event.target.value })}
            />
          </div>
          <div className="field">
            <label htmlFor="dashboardTo">{t('common.to')}</label>
            <input
              id="dashboardTo"
              type="datetime-local"
              value={filters.toUtc}
              onChange={(event) => setFilters({ ...filters, toUtc: event.target.value })}
            />
          </div>
          <div className="field">
            <label htmlFor="dashboardEntityType">{t('dashboard.entityTypeFilter')}</label>
            <input
              id="dashboardEntityType"
              type="text"
              value={filters.entityTypeId}
              onChange={(event) => setFilters({ ...filters, entityTypeId: event.target.value })}
            />
          </div>
        </div>
      </section>

      <div className="metric-grid">
        <MetricCard label={t('dashboard.totalInventory')} value={summary.totalInventoryItems} />
        <MetricCard label={t('dashboard.activeInventory')} value={summary.activeInventoryItems} />
        <MetricCard
          label={t('dashboard.executionsThisMonth')}
          value={summary.preventiveExecutionsThisMonth}
        />
        <MetricCard
          label={t('dashboard.pendingValidation')}
          value={summary.pendingValidationExecutions}
          detail={t('dashboard.submittedExecutions')}
        />
        <MetricCard label={t('dashboard.approved')} value={summary.approvedPreventiveExecutions} />
        <MetricCard label={t('dashboard.rejected')} value={summary.rejectedPreventiveExecutions} />
        <MetricCard label={t('dashboard.reworkRequested')} value={summary.reworkRequestedPreventiveExecutions} />
        <MetricCard label={t('dashboard.activeEntityTypes')} value={summary.activeEntityTypes} />
        <MetricCard label={t('dashboard.activeTemplates')} value={summary.activePreventiveTemplates} />
      </div>

      <div className="quick-actions">
        <Link className="button" to={routePaths.preventiveValidations}>
          {t('dashboard.openValidationQueue')}
        </Link>
        <Link className="button--secondary" to={routePaths.preventiveExecutions}>
          {t('dashboard.reviewExecutions')}
        </Link>
        <Link className="button--secondary" to={routePaths.inventory}>
          {t('dashboard.viewInventory')}
        </Link>
      </div>

      <div className="chart-grid">
        <SimpleBarChart
          emptyMessage={t('dashboard.noExecutionActivity')}
          points={charts.executionsByMonth}
          title={t('dashboard.executionsByMonth')}
        />
        <SimpleBarChart
          emptyMessage={t('dashboard.noValidationOutcomes')}
          points={localizePoints(charts.validationResultsByStatus)}
          title={t('dashboard.validationResultsByStatus')}
        />
        <SimpleBarChart
          emptyMessage={t('dashboard.noInventory')}
          points={localizePoints(charts.inventoryByEntityType)}
          title={t('dashboard.inventoryByEntityType')}
        />
        <SimpleBarChart
          emptyMessage={t('dashboard.noExecutions')}
          points={localizePoints(charts.executionsByEntityType)}
          title={t('dashboard.executionsByEntityType')}
        />
      </div>
    </section>
  )
}
