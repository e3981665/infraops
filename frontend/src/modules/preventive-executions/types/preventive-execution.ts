export interface PreventiveExecutionOption {
  id: string
  value: string
  label: string
  displayOrder: number
}

export interface PreventiveExecutionTemplateItem {
  id: string
  sourceChecklistItemId: string
  itemKey: string
  label: string
  itemType: 'yesNo' | 'text' | 'numeric' | 'select'
  displayOrder: number
  isRequired: boolean
  helpText: string | null
  isCritical: boolean
  requiresCommentOnFailure: boolean
  requiresPhotoOnFailure: boolean
  minimumValue: number | null
  maximumValue: number | null
  options: PreventiveExecutionOption[]
}

export interface PreventiveExecutionTemplateSection {
  id: string
  sourceTemplateSectionId: string
  title: string
  displayOrder: number
  checklistItems: PreventiveExecutionTemplateItem[]
}

export interface PreventiveExecutionAnswer {
  id: string
  itemKey: string
  value: string | null
  comment: string | null
}

export interface PreventiveValidationRecord {
  id: string
  actionType: 'approved' | 'rejected' | 'reworkRequested'
  validatorUserId: string
  createdAtUtc: string
  comment: string | null
}

export interface PreventiveExecutionDetails {
  id: string
  inventoryItemId: string
  inventoryItemDisplayName: string
  preventiveTemplateId: string
  preventiveTemplateName: string
  preventiveTemplateCode: string
  entityTypeId: string
  entityTypeName: string
  entityTypeCode: string
  regionId: string
  regionName: string
  siteId: string
  siteName: string
  status: string
  createdBy: string
  updatedBy: string
  submittedBy: string | null
  createdAtUtc: string
  updatedAtUtc: string
  submittedAtUtc: string | null
  templateSections: PreventiveExecutionTemplateSection[]
  answers: PreventiveExecutionAnswer[]
  validationHistory: PreventiveValidationRecord[]
}

export interface PreventiveExecutionSummary {
  id: string
  inventoryItemId: string
  inventoryItemDisplayName: string
  preventiveTemplateId: string
  preventiveTemplateName: string
  entityTypeId: string
  entityTypeName: string
  regionId: string
  regionName: string
  siteId: string
  siteName: string
  status: string
  createdBy: string
  updatedBy: string
  submittedBy: string | null
  createdAtUtc: string
  updatedAtUtc: string
  submittedAtUtc: string | null
}

export interface PreventiveExecutionFormDefinition {
  inventoryItemId: string
  inventoryItemDisplayName: string
  entityTypeId: string
  entityTypeName: string
  entityTypeCode: string
  preventiveTemplateId: string
  preventiveTemplateName: string
  preventiveTemplateCode: string
  sections: PreventiveExecutionTemplateSection[]
}

export interface PreventiveExecutionAnswerInput {
  itemKey: string
  value: string | null
  comment: string | null
}

export interface PreventiveExecutionListFilters {
  status: string
  entityTypeId: string
  inventoryItemId: string
  siteId: string
  regionId: string
  createdByCurrentUser: boolean
  search: string
}
