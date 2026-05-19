import { request, requireApiPathId } from '@/shared/api/http-client'
import type {
  PreventiveValidationActionInput,
  PreventiveValidationDetail,
  PreventiveValidationListFilters,
  PreventiveValidationSummary,
} from '@/modules/preventive-validations/types/preventive-validation'

function buildListPath(filters: PreventiveValidationListFilters) {
  const searchParams = new URLSearchParams()

  if (filters.status) searchParams.set('status', filters.status)
  if (filters.entityTypeId) searchParams.set('entityTypeId', filters.entityTypeId)
  if (filters.inventoryItemId) searchParams.set('inventoryItemId', filters.inventoryItemId)
  if (filters.siteId) searchParams.set('siteId', filters.siteId)
  if (filters.regionId) searchParams.set('regionId', filters.regionId)
  if (filters.submittedBy) searchParams.set('submittedBy', filters.submittedBy)
  if (filters.search) searchParams.set('search', filters.search)

  const query = searchParams.toString()

  return query ? `/api/preventive-validations?${query}` : '/api/preventive-validations'
}

export const preventiveValidationsClient = {
  list(filters: PreventiveValidationListFilters, accessToken: string) {
    return request<PreventiveValidationSummary[]>(buildListPath(filters), { accessToken })
  },
  getById(preventiveExecutionId: string, accessToken: string) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveValidationDetail>(
      `/api/preventive-validations/${id}`,
      { accessToken },
    )
  },
  approve(preventiveExecutionId: string, input: PreventiveValidationActionInput, accessToken: string) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveValidationDetail>(
      `/api/preventive-validations/${id}/approve`,
      {
        method: 'POST',
        accessToken,
        body: { comment: input.comment ?? null },
      },
    )
  },
  reject(preventiveExecutionId: string, input: PreventiveValidationActionInput, accessToken: string) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveValidationDetail>(
      `/api/preventive-validations/${id}/reject`,
      {
        method: 'POST',
        accessToken,
        body: { reason: input.reason },
      },
    )
  },
  requestRework(
    preventiveExecutionId: string,
    input: PreventiveValidationActionInput,
    accessToken: string,
  ) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveValidationDetail>(
      `/api/preventive-validations/${id}/request-rework`,
      {
        method: 'POST',
        accessToken,
        body: { reason: input.reason },
      },
    )
  },
}
