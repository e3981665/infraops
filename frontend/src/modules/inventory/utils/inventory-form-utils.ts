import type { InventoryItemFormValues } from '@/modules/inventory/schemas/inventory-form-schema'
import type {
  InventoryFormDefinition,
  InventoryFormMetadata,
  InventoryItemDetails,
  InventorySiteOption,
} from '@/modules/inventory/types/inventory'

function normalizeTextValue(value: string) {
  return value.trim()
}

export function createDefaultInventoryItemFormValues(
  metadata?: InventoryFormMetadata,
): InventoryItemFormValues {
  return {
    entityTypeId: '',
    regionId: '',
    siteId: '',
    displayName: '',
    status: metadata?.statuses[0]?.code ?? 'operational',
    installationDate: '',
    attributeValues: {},
  }
}

export function mapInventoryItemToFormValues(
  inventoryItem: InventoryItemDetails,
): InventoryItemFormValues {
  return {
    entityTypeId: inventoryItem.entityTypeId,
    regionId: inventoryItem.regionId,
    siteId: inventoryItem.siteId,
    displayName: inventoryItem.displayName,
    status: inventoryItem.status,
    installationDate: inventoryItem.installationDate ?? '',
    attributeValues: inventoryItem.attributeValues.reduce<Record<string, string>>(
      (values, attributeValue) => {
        values[attributeValue.fieldKey] = attributeValue.value
        return values
      },
      {},
    ),
  }
}

export function alignAttributeValuesWithDefinition(
  formDefinition: InventoryFormDefinition | null,
  currentValues: Record<string, string>,
) {
  if (!formDefinition) {
    return {}
  }

  return formDefinition.fieldDefinitions.reduce<Record<string, string>>((values, fieldDefinition) => {
    values[fieldDefinition.fieldKey] = currentValues[fieldDefinition.fieldKey] ?? ''
    return values
  }, {})
}

export function areAttributeValueMapsEqual(
  left: Record<string, string>,
  right: Record<string, string>,
) {
  const leftKeys = Object.keys(left)
  const rightKeys = Object.keys(right)

  if (leftKeys.length !== rightKeys.length) {
    return false
  }

  return leftKeys.every((key) => left[key] === right[key])
}

export function getSitesForRegion(
  sites: InventorySiteOption[],
  regionId: string,
) {
  if (!regionId) {
    return sites
  }

  return sites.filter((site) => site.regionId === regionId)
}

function buildAttributeValuePayload(
  values: InventoryItemFormValues,
  formDefinition: InventoryFormDefinition,
) {
  return [...formDefinition.fieldDefinitions]
    .sort((left, right) => left.displayOrder - right.displayOrder)
    .map((fieldDefinition) => {
      const rawValue = values.attributeValues[fieldDefinition.fieldKey] ?? ''
      const normalizedValue = normalizeTextValue(rawValue)

      return {
        fieldKey: fieldDefinition.fieldKey,
        value: normalizedValue || null,
      }
    })
}

export function mapInventoryItemFormToCreateRequest(
  values: InventoryItemFormValues,
  formDefinition: InventoryFormDefinition,
) {
  return {
    entityTypeId: values.entityTypeId,
    regionId: values.regionId,
    siteId: values.siteId,
    displayName: normalizeTextValue(values.displayName),
    status: values.status,
    installationDate: values.installationDate || null,
    attributeValues: buildAttributeValuePayload(values, formDefinition),
  }
}

export function mapInventoryItemFormToUpdateRequest(
  values: InventoryItemFormValues,
  formDefinition: InventoryFormDefinition,
) {
  return {
    regionId: values.regionId,
    siteId: values.siteId,
    displayName: normalizeTextValue(values.displayName),
    status: values.status,
    installationDate: values.installationDate || null,
    attributeValues: buildAttributeValuePayload(values, formDefinition),
  }
}
