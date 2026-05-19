export type EntityFieldType =
  | 'text'
  | 'textarea'
  | 'number'
  | 'decimal'
  | 'boolean'
  | 'date'
  | 'select'

export interface EntityFieldOption {
  id: string
  value: string
  label: string
  displayOrder: number
}

export interface EntityFieldDefinition {
  id: string
  fieldKey: string
  displayLabel: string
  fieldType: EntityFieldType
  displayOrder: number
  isRequired: boolean
  isActive: boolean
  placeholder: string | null
  helpText: string | null
  options: EntityFieldOption[]
}

export interface EntityTypeSummary {
  id: string
  name: string
  code: string
  description: string | null
  isActive: boolean
  fieldCount: number
}

export interface EntityTypeDetails {
  id: string
  name: string
  code: string
  description: string | null
  isActive: boolean
  fieldDefinitions: EntityFieldDefinition[]
}
