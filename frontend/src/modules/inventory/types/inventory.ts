import type { EntityFieldType } from '@/modules/entity-types/types/entity-type'

export type InventoryFieldType = EntityFieldType

export interface InventoryLookupOption {
  id: string
  code: string
  name: string
}

export interface InventorySiteOption extends InventoryLookupOption {
  regionId: string
}

export interface InventoryStatusOption {
  code: string
  label: string
}

export interface InventoryFormFieldOption {
  id: string
  value: string
  label: string
  displayOrder: number
}

export interface InventoryFormFieldDefinition {
  id: string
  fieldKey: string
  displayLabel: string
  fieldType: InventoryFieldType
  displayOrder: number
  isRequired: boolean
  placeholder: string | null
  helpText: string | null
  options: InventoryFormFieldOption[]
}

export interface InventoryFormDefinition {
  entityTypeId: string
  entityTypeName: string
  entityTypeCode: string
  fieldDefinitions: InventoryFormFieldDefinition[]
}

export interface InventoryFormMetadata {
  entityTypes: InventoryLookupOption[]
  regions: InventoryLookupOption[]
  sites: InventorySiteOption[]
  statuses: InventoryStatusOption[]
}

export interface InventoryAttributeValue {
  entityFieldDefinitionId: string
  fieldKey: string
  displayLabel: string
  fieldType: InventoryFieldType
  displayOrder: number
  value: string
}

export interface InventoryItemSummary {
  id: string
  entityTypeId: string
  entityTypeName: string
  regionId: string
  regionName: string
  siteId: string
  siteName: string
  displayName: string
  status: string
  installationDate: string | null
  isActive: boolean
}

export interface InventoryItemDetails extends InventoryItemSummary {
  entityTypeCode: string
  createdBy: string
  updatedBy: string
  createdAtUtc: string
  updatedAtUtc: string
  attributeValues: InventoryAttributeValue[]
}

export interface InventoryListFilters {
  entityTypeId?: string
  status?: string
  siteId?: string
  regionId?: string
  isActive?: string
  search?: string
}
