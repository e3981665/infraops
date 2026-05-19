import { request, requireApiPathId } from '@/shared/api/http-client'
import type {
  PreventiveExecutionAnswerInput,
  PreventiveExecutionDetails,
  PreventiveExecutionFormDefinition,
  PreventiveExecutionListFilters,
  PreventiveExecutionSummary,
} from '@/modules/preventive-executions/types/preventive-execution'

function buildListPath(filters: PreventiveExecutionListFilters) {
  const searchParams = new URLSearchParams()

  if (filters.status) searchParams.set('status', filters.status)
  if (filters.entityTypeId) searchParams.set('entityTypeId', filters.entityTypeId)
  if (filters.inventoryItemId) searchParams.set('inventoryItemId', filters.inventoryItemId)
  if (filters.siteId) searchParams.set('siteId', filters.siteId)
  if (filters.regionId) searchParams.set('regionId', filters.regionId)
  if (filters.createdByCurrentUser) searchParams.set('createdByCurrentUser', 'true')
  if (filters.search) searchParams.set('search', filters.search)

  const query = searchParams.toString()

  return query ? `/api/preventive-executions?${query}` : '/api/preventive-executions'
}

export const preventiveExecutionsClient = {
  list(filters: PreventiveExecutionListFilters, accessToken: string) {
    return request<PreventiveExecutionSummary[]>(buildListPath(filters), { accessToken })
  },
  getById(preventiveExecutionId: string, accessToken: string) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveExecutionDetails>(`/api/preventive-executions/${id}`, {
      accessToken,
    })
  },
  getFormDefinition(inventoryItemId: string, accessToken: string) {
    const id = requireApiPathId(inventoryItemId, 'Inventory item id')

    return request<PreventiveExecutionFormDefinition>(
      `/api/preventive-executions/form-definition/${id}`,
      { accessToken },
    )
  },
  start(inventoryItemId: string, accessToken: string) {
    requireApiPathId(inventoryItemId, 'Inventory item id')

    return request<PreventiveExecutionDetails>('/api/preventive-executions/start', {
      method: 'POST',
      accessToken,
      body: { inventoryItemId },
    })
  },
  saveDraft(
    preventiveExecutionId: string,
    answers: PreventiveExecutionAnswerInput[],
    accessToken: string,
  ) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveExecutionDetails>(
      `/api/preventive-executions/${id}/draft`,
      {
        method: 'PUT',
        accessToken,
        body: { answers },
      },
    )
  },
  submit(
    preventiveExecutionId: string,
    answers: PreventiveExecutionAnswerInput[],
    accessToken: string,
  ) {
    const id = requireApiPathId(preventiveExecutionId, 'Preventive execution id')

    return request<PreventiveExecutionDetails>(
      `/api/preventive-executions/${id}/submit`,
      {
        method: 'POST',
        accessToken,
        body: { answers },
      },
    )
  },
}
