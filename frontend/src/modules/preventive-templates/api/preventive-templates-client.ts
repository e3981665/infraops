import { request, requireApiPathId } from '@/shared/api/http-client'
import type {
  PreventiveTemplateDetails,
  PreventiveTemplateFormMetadata,
  PreventiveTemplateListFilters,
  PreventiveTemplateSummary,
} from '@/modules/preventive-templates/types/preventive-template'
import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import {
  mapPreventiveTemplateFormToCreateRequest,
  mapPreventiveTemplateFormToUpdateRequest,
} from '@/modules/preventive-templates/utils/preventive-template-form-utils'

function buildPreventiveTemplateListPath(filters: PreventiveTemplateListFilters) {
  const searchParams = new URLSearchParams()

  if (filters.entityTypeId) {
    searchParams.set('entityTypeId', filters.entityTypeId)
  }

  if (filters.isActive) {
    searchParams.set('isActive', filters.isActive)
  }

  if (filters.search) {
    searchParams.set('search', filters.search)
  }

  const query = searchParams.toString()

  return query ? `/api/preventive-templates?${query}` : '/api/preventive-templates'
}

export const preventiveTemplatesClient = {
  list(filters: PreventiveTemplateListFilters, accessToken: string) {
    return request<PreventiveTemplateSummary[]>(buildPreventiveTemplateListPath(filters), {
      accessToken,
    })
  },
  getById(preventiveTemplateId: string, accessToken: string) {
    const id = requireApiPathId(preventiveTemplateId, 'Preventive template id')

    return request<PreventiveTemplateDetails>(`/api/preventive-templates/${id}`, {
      accessToken,
    })
  },
  getFormMetadata(accessToken: string) {
    return request<PreventiveTemplateFormMetadata>('/api/preventive-templates/form-metadata', {
      accessToken,
    })
  },
  listByEntityType(entityTypeId: string, accessToken: string) {
    const id = requireApiPathId(entityTypeId, 'Entity type id')

    return request<PreventiveTemplateDetails[]>(`/api/preventive-templates/by-entity-type/${id}`, {
      accessToken,
    })
  },
  create(values: PreventiveTemplateFormValues, accessToken: string) {
    return request<PreventiveTemplateDetails>('/api/preventive-templates', {
      method: 'POST',
      accessToken,
      body: mapPreventiveTemplateFormToCreateRequest(values),
    })
  },
  update(preventiveTemplateId: string, values: PreventiveTemplateFormValues, accessToken: string) {
    const id = requireApiPathId(preventiveTemplateId, 'Preventive template id')

    return request<PreventiveTemplateDetails>(`/api/preventive-templates/${id}`, {
      method: 'PUT',
      accessToken,
      body: mapPreventiveTemplateFormToUpdateRequest(values),
    })
  },
  activate(preventiveTemplateId: string, accessToken: string) {
    const id = requireApiPathId(preventiveTemplateId, 'Preventive template id')

    return request<void>(`/api/preventive-templates/${id}/activate`, {
      method: 'POST',
      accessToken,
    })
  },
  deactivate(preventiveTemplateId: string, accessToken: string) {
    const id = requireApiPathId(preventiveTemplateId, 'Preventive template id')

    return request<void>(`/api/preventive-templates/${id}/deactivate`, {
      method: 'POST',
      accessToken,
    })
  },
}
