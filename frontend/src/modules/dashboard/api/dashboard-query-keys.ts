import type { DashboardFilters } from '@/modules/dashboard/types/dashboard'

export const dashboardQueryKeys = {
  all: ['dashboard'] as const,
  summary: (filters: DashboardFilters) => [...dashboardQueryKeys.all, 'summary', filters] as const,
  charts: (filters: DashboardFilters) => [...dashboardQueryKeys.all, 'charts', filters] as const,
}
