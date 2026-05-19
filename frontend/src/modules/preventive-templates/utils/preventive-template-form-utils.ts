import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import type {
  PreventiveTemplateDetails,
  PreventiveTemplateEntityTypeOption,
} from '@/modules/preventive-templates/types/preventive-template'

function normalizeWhitespace(value: string) {
  return value.trim()
}

export function normalizePreventiveTemplateCode(value: string) {
  return value
    .trim()
    .toLowerCase()
    .replace(/[\s_]+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')
}

export function normalizeChecklistItemKey(value: string) {
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

export function normalizeChecklistOptionValue(value: string) {
  return normalizePreventiveTemplateCode(value)
}

export function createDefaultChecklistOption(displayOrder: number) {
  return {
    value: '',
    label: '',
    displayOrder,
  }
}

export function createDefaultChecklistItem(displayOrder: number) {
  return {
    itemKey: '',
    label: '',
    itemType: 'yesNo' as const,
    displayOrder,
    isRequired: false,
    isActive: true,
    helpText: '',
    isCritical: false,
    requiresCommentOnFailure: false,
    requiresPhotoOnFailure: false,
    minimumValue: '',
    maximumValue: '',
    options: [],
  }
}

export function createDefaultSection(displayOrder: number) {
  return {
    title: '',
    displayOrder,
    isActive: true,
    checklistItems: [createDefaultChecklistItem(1)],
  }
}

export function createDefaultPreventiveTemplateFormValues(
  metadata?: { entityTypes: PreventiveTemplateEntityTypeOption[] },
): PreventiveTemplateFormValues {
  return {
    entityTypeId: metadata?.entityTypes[0]?.id ?? '',
    name: '',
    code: '',
    description: '',
    sections: [createDefaultSection(1)],
  }
}

export function mapPreventiveTemplateToFormValues(
  preventiveTemplate: PreventiveTemplateDetails,
): PreventiveTemplateFormValues {
  return {
    entityTypeId: preventiveTemplate.entityTypeId,
    name: preventiveTemplate.name,
    code: preventiveTemplate.code,
    description: preventiveTemplate.description ?? '',
    sections: preventiveTemplate.sections.map((section) => ({
      id: section.id,
      title: section.title,
      displayOrder: section.displayOrder,
      isActive: section.isActive,
      checklistItems: section.checklistItems.map((item) => ({
        id: item.id,
        itemKey: item.itemKey,
        label: item.label,
        itemType: item.itemType,
        displayOrder: item.displayOrder,
        isRequired: item.isRequired,
        isActive: item.isActive,
        helpText: item.helpText ?? '',
        isCritical: item.isCritical,
        requiresCommentOnFailure: item.requiresCommentOnFailure,
        requiresPhotoOnFailure: item.requiresPhotoOnFailure,
        minimumValue: item.minimumValue?.toString() ?? '',
        maximumValue: item.maximumValue?.toString() ?? '',
        options: item.options.map((option) => ({
          id: option.id,
          value: option.value,
          label: option.label,
          displayOrder: option.displayOrder,
        })),
      })),
    })),
  }
}

export function ensureTemplateEntityTypeOption(
  entityTypes: PreventiveTemplateEntityTypeOption[],
  entityType: PreventiveTemplateEntityTypeOption,
) {
  if (entityTypes.some((option) => option.id === entityType.id)) {
    return entityTypes
  }

  return [entityType, ...entityTypes].sort((left, right) => left.name.localeCompare(right.name))
}

function mapChecklistItems(items: PreventiveTemplateFormValues['sections'][number]['checklistItems']) {
  return items.map((item) => ({
    id: item.id,
    itemKey: normalizeChecklistItemKey(item.itemKey),
    label: normalizeWhitespace(item.label),
    itemType: item.itemType,
    displayOrder: item.displayOrder,
    isRequired: item.isRequired,
    isActive: item.isActive,
    helpText: normalizeWhitespace(item.helpText) || null,
    isCritical: item.isCritical,
    requiresCommentOnFailure: item.requiresCommentOnFailure,
    requiresPhotoOnFailure: item.requiresPhotoOnFailure,
    minimumValue: item.itemType === 'numeric' && item.minimumValue.trim() ? Number(item.minimumValue) : null,
    maximumValue: item.itemType === 'numeric' && item.maximumValue.trim() ? Number(item.maximumValue) : null,
    options:
      item.itemType === 'select'
        ? item.options.map((option) => ({
            id: option.id,
            value: normalizeChecklistOptionValue(option.value),
            label: normalizeWhitespace(option.label),
            displayOrder: option.displayOrder,
          }))
        : [],
  }))
}

export function mapPreventiveTemplateFormToCreateRequest(values: PreventiveTemplateFormValues) {
  return {
    entityTypeId: values.entityTypeId,
    name: normalizeWhitespace(values.name),
    code: normalizePreventiveTemplateCode(values.code),
    description: normalizeWhitespace(values.description) || null,
    sections: values.sections.map((section) => ({
      id: section.id,
      title: normalizeWhitespace(section.title),
      displayOrder: section.displayOrder,
      isActive: section.isActive,
      checklistItems: mapChecklistItems(section.checklistItems),
    })),
  }
}

export function mapPreventiveTemplateFormToUpdateRequest(values: PreventiveTemplateFormValues) {
  return {
    name: normalizeWhitespace(values.name),
    code: normalizePreventiveTemplateCode(values.code),
    description: normalizeWhitespace(values.description) || null,
    sections: values.sections.map((section) => ({
      id: section.id,
      title: normalizeWhitespace(section.title),
      displayOrder: section.displayOrder,
      isActive: section.isActive,
      checklistItems: mapChecklistItems(section.checklistItems),
    })),
  }
}
