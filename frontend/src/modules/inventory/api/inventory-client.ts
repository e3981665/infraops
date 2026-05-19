import { request, requireApiPathId } from '@/shared/api/http-client'
import type {
  InventoryFormDefinition,
  InventoryFormMetadata,
  InventoryItemDetails,
  InventoryItemSummary,
  InventoryListFilters,
} from '@/modules/inventory/types/inventory'
import type { InventoryItemFormValues } from '@/modules/inventory/schemas/inventory-form-schema'
import {
  mapInventoryItemFormToCreateRequest,
  mapInventoryItemFormToUpdateRequest,
} from '@/modules/inventory/utils/inventory-form-utils'

function buildInventoryListPath(filters: InventoryListFilters) {
  const searchParams = new URLSearchParams()

  if (filters.entityTypeId) {
    searchParams.set('entityTypeId', filters.entityTypeId)
  }

  if (filters.status) {
    searchParams.set('status', filters.status)
  }

  if (filters.siteId) {
    searchParams.set('siteId', filters.siteId)
  }

  if (filters.regionId) {
    searchParams.set('regionId', filters.regionId)
  }

  if (filters.isActive) {
    searchParams.set('isActive', filters.isActive)
  }

  if (filters.search) {
    searchParams.set('search', filters.search)
  }

  const query = searchParams.toString()

  return query ? `/api/inventory?${query}` : '/api/inventory'
}

export const inventoryClient = {
  list(filters: InventoryListFilters, accessToken: string) {
    return request<InventoryItemSummary[]>(buildInventoryListPath(filters), {
      accessToken,
    })
  },
  getById(inventoryItemId: string, accessToken: string) {
    const id = requireApiPathId(inventoryItemId, 'Inventory item id')

    return request<InventoryItemDetails>(`/api/inventory/${id}`, {
      accessToken,
    })
  },
  getFormMetadata(accessToken: string) {
    return request<InventoryFormMetadata>('/api/inventory/form-metadata', {
      accessToken,
    })
  },
  getFormDefinition(entityTypeId: string, accessToken: string) {
    const id = requireApiPathId(entityTypeId, 'Entity type id')

    return request<InventoryFormDefinition>(`/api/inventory/form-definition/${id}`, {
      accessToken,
    })
  },
  create(
    values: InventoryItemFormValues,
    formDefinition: InventoryFormDefinition,
    accessToken: string,
  ) {
    return request<InventoryItemDetails>('/api/inventory', {
      method: 'POST',
      accessToken,
      body: mapInventoryItemFormToCreateRequest(values, formDefinition),
    })
  },
  update(
    inventoryItemId: string,
    values: InventoryItemFormValues,
    formDefinition: InventoryFormDefinition,
    accessToken: string,
  ) {
    const id = requireApiPathId(inventoryItemId, 'Inventory item id')

    return request<InventoryItemDetails>(`/api/inventory/${id}`, {
      method: 'PUT',
      accessToken,
      body: mapInventoryItemFormToUpdateRequest(values, formDefinition),
    })
  },
  activate(inventoryItemId: string, accessToken: string) {
    const id = requireApiPathId(inventoryItemId, 'Inventory item id')

    return request<void>(`/api/inventory/${id}/activate`, {
      method: 'POST',
      accessToken,
    })
  },
  deactivate(inventoryItemId: string, accessToken: string) {
    const id = requireApiPathId(inventoryItemId, 'Inventory item id')

    return request<void>(`/api/inventory/${id}/deactivate`, {
      method: 'POST',
      accessToken,
    })
  },
}
