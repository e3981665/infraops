export interface DashboardMetricPoint {
  label: string
  value: number
}

export interface DashboardSummary {
  totalInventoryItems: number
  activeInventoryItems: number
  preventiveExecutionsThisMonth: number
  pendingValidationExecutions: number
  approvedPreventiveExecutions: number
  rejectedPreventiveExecutions: number
  reworkRequestedPreventiveExecutions: number
  activeEntityTypes: number
  activePreventiveTemplates: number
}

export interface ExecutionMetrics {
  totalExecutions: number
  draftExecutions: number
  submittedExecutions: number
  approvedExecutions: number
  rejectedExecutions: number
  reworkRequestedExecutions: number
  executionsByEntityType: DashboardMetricPoint[]
}

export interface ValidationMetrics {
  pendingValidation: number
  approved: number
  rejected: number
  reworkRequested: number
  resultsByStatus: DashboardMetricPoint[]
}

export interface InventoryMetrics {
  totalInventoryItems: number
  activeInventoryItems: number
  inventoryByEntityType: DashboardMetricPoint[]
}

export interface DashboardCharts {
  executionsByMonth: DashboardMetricPoint[]
  validationResultsByStatus: DashboardMetricPoint[]
  inventoryByEntityType: DashboardMetricPoint[]
  executionsByEntityType: DashboardMetricPoint[]
}

export interface DashboardFilters {
  regionId: string
  siteId: string
  entityTypeId: string
  fromUtc: string
  toUtc: string
}
