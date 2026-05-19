import { z } from 'zod'
import type { InventoryFormDefinition } from '@/modules/inventory/types/inventory'

const wholeNumberPattern = /^-?\d+$/
const decimalPattern = /^-?\d+(?:\.\d+)?$/
const datePattern = /^\d{4}-\d{2}-\d{2}$/

export function createInventoryItemFormSchema(formDefinition: InventoryFormDefinition | null) {
  return z
    .object({
      entityTypeId: z.string().uuid('Entity type is required.'),
      regionId: z.string().uuid('Region is required.'),
      siteId: z.string().uuid('Site is required.'),
      displayName: z
        .string()
        .trim()
        .min(1, 'Display name is required.')
        .max(200, 'Display name cannot exceed 200 characters.'),
      status: z.string().trim().min(1, 'Status is required.'),
      installationDate: z.string().optional().or(z.literal('')),
      attributeValues: z.record(z.string()),
    })
    .superRefine((values, context) => {
      if (!formDefinition) {
        return
      }

      formDefinition.fieldDefinitions.forEach((fieldDefinition) => {
        const rawValue = values.attributeValues[fieldDefinition.fieldKey] ?? ''
        const normalizedValue = rawValue.trim()

        if (!normalizedValue) {
          if (fieldDefinition.isRequired) {
            context.addIssue({
              code: z.ZodIssueCode.custom,
              message: `${fieldDefinition.displayLabel} is required.`,
              path: ['attributeValues', fieldDefinition.fieldKey],
            })
          }

          return
        }

        if (
          fieldDefinition.fieldType === 'number' &&
          !wholeNumberPattern.test(normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: `${fieldDefinition.displayLabel} must be a whole number.`,
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'decimal' &&
          !decimalPattern.test(normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: `${fieldDefinition.displayLabel} must be a decimal value.`,
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'boolean' &&
          normalizedValue !== 'true' &&
          normalizedValue !== 'false'
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: `${fieldDefinition.displayLabel} must be true or false.`,
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'date' &&
          !datePattern.test(normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: `${fieldDefinition.displayLabel} must use the yyyy-MM-dd format.`,
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'select' &&
          !fieldDefinition.options.some((option) => option.value === normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: `${fieldDefinition.displayLabel} must use one of the configured options.`,
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }
      })
    })
}

export type InventoryItemFormValues = z.infer<
  ReturnType<typeof createInventoryItemFormSchema>
>
