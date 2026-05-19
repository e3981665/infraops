import { z } from 'zod'
import type { InventoryFormDefinition } from '@/modules/inventory/types/inventory'
import {
  defaultSchemaTranslate,
  type SchemaTranslate,
} from '@/shared/i18n/schema-translation'

const wholeNumberPattern = /^-?\d+$/
const decimalPattern = /^-?\d+(?:\.\d+)?$/
const datePattern = /^\d{4}-\d{2}-\d{2}$/

export function createInventoryItemFormSchema(
  formDefinition: InventoryFormDefinition | null,
  t: SchemaTranslate = defaultSchemaTranslate,
) {
  return z
    .object({
      entityTypeId: z.string().uuid(t('inventory.validation.entityTypeRequired')),
      regionId: z.string().uuid(t('inventory.validation.regionRequired')),
      siteId: z.string().uuid(t('inventory.validation.siteRequired')),
      displayName: z
        .string()
        .trim()
        .min(1, t('inventory.validation.displayNameRequired'))
        .max(200, t('inventory.validation.displayNameMax')),
      status: z.string().trim().min(1, t('inventory.validation.statusRequired')),
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
              message: t('inventory.validation.dynamicRequired', {
                field: fieldDefinition.displayLabel,
              }),
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
            message: t('inventory.validation.dynamicWholeNumber', {
              field: fieldDefinition.displayLabel,
            }),
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'decimal' &&
          !decimalPattern.test(normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: t('inventory.validation.dynamicDecimal', {
              field: fieldDefinition.displayLabel,
            }),
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
            message: t('inventory.validation.dynamicBoolean', {
              field: fieldDefinition.displayLabel,
            }),
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'date' &&
          !datePattern.test(normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: t('inventory.validation.dynamicDate', {
              field: fieldDefinition.displayLabel,
            }),
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }

        if (
          fieldDefinition.fieldType === 'select' &&
          !fieldDefinition.options.some((option) => option.value === normalizedValue)
        ) {
          context.addIssue({
            code: z.ZodIssueCode.custom,
            message: t('inventory.validation.dynamicSelectOption', {
              field: fieldDefinition.displayLabel,
            }),
            path: ['attributeValues', fieldDefinition.fieldKey],
          })
        }
      })
    })
}

export type InventoryItemFormValues = z.infer<
  ReturnType<typeof createInventoryItemFormSchema>
>
