import { request, requireApiPathId } from '@/shared/api/http-client'
import type {
  EntityTypeDetails,
  EntityTypeSummary,
} from '@/modules/entity-types/types/entity-type'
import type { EntityTypeFormValues } from '@/modules/entity-types/schemas/entity-type-form-schema'
import { mapEntityTypeFormToRequest } from '@/modules/entity-types/utils/entity-type-form-utils'

export const entityTypesClient = {
  list(accessToken: string) {
    return request<EntityTypeSummary[]>('/api/entity-types', {
      accessToken,
    })
  },
  getById(entityTypeId: string, accessToken: string) {
    const id = requireApiPathId(entityTypeId, 'Entity type id')

    return request<EntityTypeDetails>(`/api/entity-types/${id}`, {
      accessToken,
    })
  },
  create(values: EntityTypeFormValues, accessToken: string) {
    return request<EntityTypeDetails>('/api/entity-types', {
      method: 'POST',
      accessToken,
      body: mapEntityTypeFormToRequest(values),
    })
  },
  update(entityTypeId: string, values: EntityTypeFormValues, accessToken: string) {
    const id = requireApiPathId(entityTypeId, 'Entity type id')

    return request<EntityTypeDetails>(`/api/entity-types/${id}`, {
      method: 'PUT',
      accessToken,
      body: mapEntityTypeFormToRequest(values),
    })
  },
  activate(entityTypeId: string, accessToken: string) {
    const id = requireApiPathId(entityTypeId, 'Entity type id')

    return request<void>(`/api/entity-types/${id}/activate`, {
      method: 'POST',
      accessToken,
    })
  },
  deactivate(entityTypeId: string, accessToken: string) {
    const id = requireApiPathId(entityTypeId, 'Entity type id')

    return request<void>(`/api/entity-types/${id}/deactivate`, {
      method: 'POST',
      accessToken,
    })
  },
}
