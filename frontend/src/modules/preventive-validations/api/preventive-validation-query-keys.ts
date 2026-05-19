import type { PreventiveValidationListFilters } from '@/modules/preventive-validations/types/preventive-validation'

export const preventiveValidationQueryKeys = {
  all: ['preventive-validations'] as const,
  list: (filters: PreventiveValidationListFilters) =>
    [...preventiveValidationQueryKeys.all, 'list', filters] as const,
  detail: (preventiveExecutionId: string) =>
    [...preventiveValidationQueryKeys.all, 'detail', preventiveExecutionId] as const,
}
