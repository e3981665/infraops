import type { PreventiveExecutionListFilters } from '@/modules/preventive-executions/types/preventive-execution'

export const preventiveExecutionQueryKeys = {
  all: ['preventive-executions'] as const,
  lists: () => [...preventiveExecutionQueryKeys.all, 'list'] as const,
  list: (filters: PreventiveExecutionListFilters) =>
    [...preventiveExecutionQueryKeys.lists(), filters] as const,
  detail: (preventiveExecutionId: string) =>
    [...preventiveExecutionQueryKeys.all, 'detail', preventiveExecutionId] as const,
  formDefinition: (inventoryItemId: string) =>
    [...preventiveExecutionQueryKeys.all, 'form-definition', inventoryItemId] as const,
}
