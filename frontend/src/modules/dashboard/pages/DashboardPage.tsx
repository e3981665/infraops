import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { dashboardClient } from '@/modules/dashboard/api/dashboard-client'
import { dashboardQueryKeys } from '@/modules/dashboard/api/dashboard-query-keys'
import { MetricCard } from '@/modules/dashboard/components/MetricCard'
import { SimpleBarChart } from '@/modules/dashboard/components/SimpleBarChart'
import type { DashboardFilters } from '@/modules/dashboard/types/dashboard'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
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
        <p className="hero-panel__eyebrow">Operational dashboard</p>
        <h1>Authenticated access is required.</h1>
      </section>
    )
  }

  if (summaryQuery.isLoading || chartsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Operational dashboard</p>
        <h1>Loading operational metrics.</h1>
        <p>InfraOps is aggregating inventory, execution, and validation activity.</p>
      </section>
    )
  }

  if (summaryQuery.isError || chartsQuery.isError || !summaryQuery.data || !chartsQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Operational dashboard</p>
        <h1>Dashboard metrics could not be loaded.</h1>
        <p>{summaryQuery.error?.message ?? chartsQuery.error?.message}</p>
      </section>
    )
  }

  const summary = summaryQuery.data
  const charts = chartsQuery.data

  return (
    <section className="module-panel dashboard-workspace">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Operational dashboard</p>
          <h1>Infrastructure preventive maintenance overview.</h1>
        </div>
        <p>Live operational visibility across inventory, checklist execution, and validation.</p>
      </div>

      <section className="form-section">
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="dashboardFrom">From</label>
            <input
              id="dashboardFrom"
              type="datetime-local"
              value={filters.fromUtc}
              onChange={(event) => setFilters({ ...filters, fromUtc: event.target.value })}
            />
          </div>
          <div className="field">
            <label htmlFor="dashboardTo">To</label>
            <input
              id="dashboardTo"
              type="datetime-local"
              value={filters.toUtc}
              onChange={(event) => setFilters({ ...filters, toUtc: event.target.value })}
            />
          </div>
          <div className="field">
            <label htmlFor="dashboardEntityType">Entity type id</label>
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
        <MetricCard label="Total inventory" value={summary.totalInventoryItems} />
        <MetricCard label="Active inventory" value={summary.activeInventoryItems} />
        <MetricCard
          label="Executions this month"
          value={summary.preventiveExecutionsThisMonth}
        />
        <MetricCard
          label="Pending validation"
          value={summary.pendingValidationExecutions}
          detail="Submitted executions"
        />
        <MetricCard label="Approved" value={summary.approvedPreventiveExecutions} />
        <MetricCard label="Rejected" value={summary.rejectedPreventiveExecutions} />
        <MetricCard label="Rework requested" value={summary.reworkRequestedPreventiveExecutions} />
        <MetricCard label="Active entity types" value={summary.activeEntityTypes} />
        <MetricCard label="Active templates" value={summary.activePreventiveTemplates} />
      </div>

      <div className="quick-actions">
        <Link className="button" to={routePaths.preventiveValidations}>
          Open validation queue
        </Link>
        <Link className="button--secondary" to={routePaths.preventiveExecutions}>
          Review executions
        </Link>
        <Link className="button--secondary" to={routePaths.inventory}>
          View inventory
        </Link>
      </div>

      <div className="chart-grid">
        <SimpleBarChart
          emptyMessage="No execution activity exists for the current filters."
          points={charts.executionsByMonth}
          title="Preventive executions by month"
        />
        <SimpleBarChart
          emptyMessage="No validation outcomes exist for the current filters."
          points={charts.validationResultsByStatus}
          title="Validation results by status"
        />
        <SimpleBarChart
          emptyMessage="No inventory exists for the current filters."
          points={charts.inventoryByEntityType}
          title="Inventory by entity type"
        />
        <SimpleBarChart
          emptyMessage="No executions exist for the current filters."
          points={charts.executionsByEntityType}
          title="Executions by entity type"
        />
      </div>
    </section>
  )
}
