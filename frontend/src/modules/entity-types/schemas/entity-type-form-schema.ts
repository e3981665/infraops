import { z } from 'zod'

const entityFieldOptionSchema = z.object({
  id: z.string().uuid().optional(),
  value: z
    .string()
    .trim()
    .min(1, 'Option value is required.')
    .max(80, 'Option value cannot exceed 80 characters.'),
  label: z
    .string()
    .trim()
    .min(1, 'Option label is required.')
    .max(120, 'Option label cannot exceed 120 characters.'),
  displayOrder: z.coerce
    .number()
    .int('Display order must be a whole number.')
    .positive('Display order must be greater than zero.'),
})

const entityFieldDefinitionSchema = z
  .object({
    id: z.string().uuid().optional(),
    fieldKey: z
      .string()
      .trim()
      .min(1, 'Field key is required.')
      .max(80, 'Field key cannot exceed 80 characters.')
      .regex(
        /^[a-z][A-Za-z0-9]*$/,
        'Field key must start with a lowercase letter and use only letters and numbers.',
      ),
    displayLabel: z
      .string()
      .trim()
      .min(1, 'Display label is required.')
      .max(120, 'Display label cannot exceed 120 characters.'),
    fieldType: z.enum([
      'text',
      'textarea',
      'number',
      'decimal',
      'boolean',
      'date',
      'select',
    ]),
    displayOrder: z.coerce
      .number()
      .int('Display order must be a whole number.')
      .positive('Display order must be greater than zero.'),
    isRequired: z.boolean(),
    isActive: z.boolean(),
    placeholder: z
      .string()
      .trim()
      .max(200, 'Placeholder cannot exceed 200 characters.')
      .optional()
      .or(z.literal('')),
    helpText: z
      .string()
      .trim()
      .max(500, 'Help text cannot exceed 500 characters.')
      .optional()
      .or(z.literal('')),
    options: z.array(entityFieldOptionSchema),
  })
  .superRefine((value, context) => {
    if (value.fieldType === 'select' && value.options.length === 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Select fields require at least one option.',
        path: ['options'],
      })
    }

    if (value.fieldType !== 'select' && value.options.length > 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Only select fields can define options.',
        path: ['options'],
      })
    }

    const optionValues = new Set<string>()
    const optionOrders = new Set<number>()

    value.options.forEach((option, index) => {
      const normalizedValue = option.value.trim().toLowerCase()

      if (normalizedValue && optionValues.has(normalizedValue)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Option values must be unique within a field.',
          path: ['options', index, 'value'],
        })
      }

      if (optionOrders.has(option.displayOrder)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Option display order must be unique within a field.',
          path: ['options', index, 'displayOrder'],
        })
      }

      optionValues.add(normalizedValue)
      optionOrders.add(option.displayOrder)
    })
  })

export const entityTypeFormSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(1, 'Entity type name is required.')
      .max(100, 'Entity type name cannot exceed 100 characters.'),
    code: z
      .string()
      .trim()
      .min(1, 'Entity type code is required.')
      .max(60, 'Entity type code cannot exceed 60 characters.')
      .regex(
        /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
        'Entity type code must use lowercase letters, numbers, and hyphens only.',
      ),
    description: z
      .string()
      .trim()
      .max(500, 'Description cannot exceed 500 characters.')
      .optional()
      .or(z.literal('')),
    fieldDefinitions: z.array(entityFieldDefinitionSchema),
  })
  .superRefine((value, context) => {
    const fieldKeys = new Set<string>()
    const displayOrders = new Set<number>()

    value.fieldDefinitions.forEach((fieldDefinition, index) => {
      const normalizedFieldKey = fieldDefinition.fieldKey.trim()

      if (fieldKeys.has(normalizedFieldKey)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Field keys must be unique within an entity type.',
          path: ['fieldDefinitions', index, 'fieldKey'],
        })
      }

      if (displayOrders.has(fieldDefinition.displayOrder)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Display order must be unique within an entity type.',
          path: ['fieldDefinitions', index, 'displayOrder'],
        })
      }

      fieldKeys.add(normalizedFieldKey)
      displayOrders.add(fieldDefinition.displayOrder)
    })
  })

export type EntityTypeFormValues = z.infer<typeof entityTypeFormSchema>
