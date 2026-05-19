export type PreventiveChecklistItemType = 'yesNo' | 'text' | 'numeric' | 'select'

export interface PreventiveTemplateEntityTypeOption {
  id: string
  code: string
  name: string
}

export interface PreventiveChecklistOption {
  id: string
  value: string
  label: string
  displayOrder: number
}

export interface PreventiveChecklistItem {
  id: string
  itemKey: string
  label: string
  itemType: PreventiveChecklistItemType
  displayOrder: number
  isRequired: boolean
  isActive: boolean
  helpText: string | null
  isCritical: boolean
  requiresCommentOnFailure: boolean
  requiresPhotoOnFailure: boolean
  minimumValue: number | null
  maximumValue: number | null
  options: PreventiveChecklistOption[]
}

export interface PreventiveTemplateSection {
  id: string
  title: string
  displayOrder: number
  isActive: boolean
  checklistItems: PreventiveChecklistItem[]
}

export interface PreventiveTemplateSummary {
  id: string
  entityTypeId: string
  entityTypeName: string
  name: string
  code: string
  description: string | null
  isActive: boolean
  sectionCount: number
  checklistItemCount: number
}

export interface PreventiveTemplateDetails {
  id: string
  entityTypeId: string
  entityTypeName: string
  entityTypeCode: string
  name: string
  code: string
  description: string | null
  isActive: boolean
  sections: PreventiveTemplateSection[]
}

export interface PreventiveTemplateFormMetadata {
  entityTypes: PreventiveTemplateEntityTypeOption[]
}

export interface PreventiveTemplateListFilters {
  entityTypeId?: string
  isActive?: string
  search?: string
}
