import type { EntityTypeDetails } from '@/modules/entity-types/types/entity-type'
import type { EntityTypeFormValues } from '@/modules/entity-types/schemas/entity-type-form-schema'

function normalizeWhitespace(value: string) {
  return value.trim()
}

export function normalizeEntityTypeCode(value: string) {
  return value
    .trim()
    .toLowerCase()
    .replace(/[\s_]+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')
}

export function normalizeFieldKey(value: string) {
  const segments = value
    .trim()
    .replace(/[^a-zA-Z0-9]+/g, ' ')
    .split(' ')
    .filter(Boolean)

  if (segments.length === 0) {
    return ''
  }

  const [firstSegment, ...remainingSegments] = segments

  return [
    firstSegment.toLowerCase(),
    ...remainingSegments.map((segment) => `${segment[0]!.toUpperCase()}${segment.slice(1)}`),
  ].join('')
}

export function createDefaultFieldOption(displayOrder: number) {
  return {
    value: '',
    label: '',
    displayOrder,
  }
}

export function createDefaultFieldDefinition(displayOrder: number) {
  return {
    fieldKey: '',
    displayLabel: '',
    fieldType: 'text' as const,
    displayOrder,
    isRequired: false,
    isActive: true,
    placeholder: '',
    helpText: '',
    options: [],
  }
}

export function createDefaultEntityTypeFormValues(): EntityTypeFormValues {
  return {
    name: '',
    code: '',
    description: '',
    fieldDefinitions: [createDefaultFieldDefinition(1)],
  }
}

export function mapEntityTypeToFormValues(entityType: EntityTypeDetails): EntityTypeFormValues {
  return {
    name: entityType.name,
    code: entityType.code,
    description: entityType.description ?? '',
    fieldDefinitions: entityType.fieldDefinitions.map((fieldDefinition) => ({
      id: fieldDefinition.id,
      fieldKey: fieldDefinition.fieldKey,
      displayLabel: fieldDefinition.displayLabel,
      fieldType: fieldDefinition.fieldType,
      displayOrder: fieldDefinition.displayOrder,
      isRequired: fieldDefinition.isRequired,
      isActive: fieldDefinition.isActive,
      placeholder: fieldDefinition.placeholder ?? '',
      helpText: fieldDefinition.helpText ?? '',
      options: fieldDefinition.options.map((option) => ({
        id: option.id,
        value: option.value,
        label: option.label,
        displayOrder: option.displayOrder,
      })),
    })),
  }
}

export function mapEntityTypeFormToRequest(values: EntityTypeFormValues) {
  return {
    name: normalizeWhitespace(values.name),
    code: normalizeEntityTypeCode(values.code),
    description: normalizeWhitespace(values.description ?? '') || null,
    fieldDefinitions: values.fieldDefinitions.map((fieldDefinition) => ({
      id: fieldDefinition.id,
      fieldKey: normalizeFieldKey(fieldDefinition.fieldKey),
      displayLabel: normalizeWhitespace(fieldDefinition.displayLabel),
      fieldType: fieldDefinition.fieldType,
      displayOrder: fieldDefinition.displayOrder,
      isRequired: fieldDefinition.isRequired,
      isActive: fieldDefinition.isActive,
      placeholder: normalizeWhitespace(fieldDefinition.placeholder ?? '') || null,
      helpText: normalizeWhitespace(fieldDefinition.helpText ?? '') || null,
      options:
        fieldDefinition.fieldType === 'select'
          ? fieldDefinition.options.map((option) => ({
              id: option.id,
              value: normalizeEntityTypeCode(option.value),
              label: normalizeWhitespace(option.label),
              displayOrder: option.displayOrder,
            }))
          : [],
    })),
  }
}
