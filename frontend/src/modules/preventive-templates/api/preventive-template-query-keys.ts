import type { PreventiveTemplateListFilters } from '@/modules/preventive-templates/types/preventive-template'

export const preventiveTemplateQueryKeys = {
  all: ['preventive-templates'] as const,
  lists: () => ['preventive-templates', 'list'] as const,
  list: (filters: PreventiveTemplateListFilters) =>
    ['preventive-templates', 'list', filters] as const,
  detail: (preventiveTemplateId: string) =>
    ['preventive-templates', 'detail', preventiveTemplateId] as const,
  formMetadata: ['preventive-templates', 'form-metadata'] as const,
  byEntityType: (entityTypeId: string) =>
    ['preventive-templates', 'by-entity-type', entityTypeId] as const,
}
