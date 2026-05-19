import { request } from '@/shared/api/http-client'
import type {
  DashboardCharts,
  DashboardFilters,
  DashboardSummary,
} from '@/modules/dashboard/types/dashboard'

function buildDashboardPath(path: string, filters: DashboardFilters) {
  const searchParams = new URLSearchParams()

  if (filters.regionId) searchParams.set('regionId', filters.regionId)
  if (filters.siteId) searchParams.set('siteId', filters.siteId)
  if (filters.entityTypeId) searchParams.set('entityTypeId', filters.entityTypeId)
  if (filters.fromUtc) searchParams.set('fromUtc', filters.fromUtc)
  if (filters.toUtc) searchParams.set('toUtc', filters.toUtc)

  const query = searchParams.toString()

  return query ? `${path}?${query}` : path
}

export const dashboardClient = {
  getSummary(filters: DashboardFilters, accessToken: string) {
    return request<DashboardSummary>(buildDashboardPath('/api/dashboard/summary', filters), {
      accessToken,
    })
  },
  getCharts(filters: DashboardFilters, accessToken: string) {
    return request<DashboardCharts>(buildDashboardPath('/api/dashboard/charts', filters), {
      accessToken,
    })
  },
}
