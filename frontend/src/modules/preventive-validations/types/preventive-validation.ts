import type {
  PreventiveExecutionDetails,
  PreventiveExecutionSummary,
} from '@/modules/preventive-executions/types/preventive-execution'

export interface PreventiveValidationListFilters {
  status: string
  entityTypeId: string
  inventoryItemId: string
  siteId: string
  regionId: string
  submittedBy: string
  search: string
}

export interface PreventiveValidationActionInput {
  comment?: string | null
  reason?: string
}

export type PreventiveValidationSummary = PreventiveExecutionSummary

export type PreventiveValidationDetail = PreventiveExecutionDetails
