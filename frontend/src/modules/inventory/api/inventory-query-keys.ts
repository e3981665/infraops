import type { InventoryListFilters } from '@/modules/inventory/types/inventory'

export const inventoryQueryKeys = {
  all: ['inventory'] as const,
  lists: () => ['inventory', 'list'] as const,
  list: (filters: InventoryListFilters) => ['inventory', 'list', filters] as const,
  detail: (inventoryItemId: string) => ['inventory', 'detail', inventoryItemId] as const,
  formMetadata: ['inventory', 'form-metadata'] as const,
  formDefinition: (entityTypeId: string) =>
    ['inventory', 'form-definition', entityTypeId] as const,
}
