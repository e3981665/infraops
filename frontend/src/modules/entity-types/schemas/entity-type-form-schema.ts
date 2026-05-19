import { z } from 'zod'
import {
  defaultSchemaTranslate,
  type SchemaTranslate,
} from '@/shared/i18n/schema-translation'

const createEntityFieldOptionSchema = (t: SchemaTranslate) => z.object({
  id: z.string().uuid().optional(),
  value: z
    .string()
    .trim()
    .min(1, t('entity.validation.optionValueRequired'))
    .max(80, t('entity.validation.optionValueMax')),
  label: z
    .string()
    .trim()
    .min(1, t('entity.validation.optionLabelRequired'))
    .max(120, t('entity.validation.optionLabelMax')),
  displayOrder: z.coerce
    .number()
    .int(t('entity.validation.displayOrderWholeNumber'))
    .positive(t('entity.validation.displayOrderPositive')),
})

const createEntityFieldDefinitionSchema = (t: SchemaTranslate) => z
  .object({
    id: z.string().uuid().optional(),
    fieldKey: z
      .string()
      .trim()
      .min(1, t('entity.validation.fieldKeyRequired'))
      .max(80, t('entity.validation.fieldKeyMax'))
      .regex(
        /^[a-z][A-Za-z0-9]*$/,
        t('entity.validation.fieldKeyPattern'),
      ),
    displayLabel: z
      .string()
      .trim()
      .min(1, t('entity.validation.displayLabelRequired'))
      .max(120, t('entity.validation.displayLabelMax')),
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
      .int(t('entity.validation.displayOrderWholeNumber'))
      .positive(t('entity.validation.displayOrderPositive')),
    isRequired: z.boolean(),
    isActive: z.boolean(),
    placeholder: z
      .string()
      .trim()
      .max(200, t('entity.validation.placeholderMax'))
      .optional()
      .or(z.literal('')),
    helpText: z
      .string()
      .trim()
      .max(500, t('entity.validation.helpTextMax'))
      .optional()
      .or(z.literal('')),
    options: z.array(createEntityFieldOptionSchema(t)),
  })
  .superRefine((value, context) => {
    if (value.fieldType === 'select' && value.options.length === 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('entity.validation.selectRequiresOption'),
        path: ['options'],
      })
    }

    if (value.fieldType !== 'select' && value.options.length > 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('entity.validation.onlySelectCanDefineOptions'),
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
          message: t('entity.validation.optionValuesUnique'),
          path: ['options', index, 'value'],
        })
      }

      if (optionOrders.has(option.displayOrder)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('entity.validation.optionDisplayOrderUnique'),
          path: ['options', index, 'displayOrder'],
        })
      }

      optionValues.add(normalizedValue)
      optionOrders.add(option.displayOrder)
    })
  })

export const createEntityTypeFormSchema = (t: SchemaTranslate = defaultSchemaTranslate) => z
  .object({
    name: z
      .string()
      .trim()
      .min(1, t('entity.validation.nameRequired'))
      .max(100, t('entity.validation.nameMax')),
    code: z
      .string()
      .trim()
      .min(1, t('entity.validation.codeRequired'))
      .max(60, t('entity.validation.codeMax'))
      .regex(
        /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
        t('entity.validation.codePattern'),
      ),
    description: z
      .string()
      .trim()
      .max(500, t('entity.validation.descriptionMax'))
      .optional()
      .or(z.literal('')),
    fieldDefinitions: z.array(createEntityFieldDefinitionSchema(t)),
  })
  .superRefine((value, context) => {
    const fieldKeys = new Set<string>()
    const displayOrders = new Set<number>()

    value.fieldDefinitions.forEach((fieldDefinition, index) => {
      const normalizedFieldKey = fieldDefinition.fieldKey.trim()

      if (fieldKeys.has(normalizedFieldKey)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('entity.validation.fieldKeysUnique'),
          path: ['fieldDefinitions', index, 'fieldKey'],
        })
      }

      if (displayOrders.has(fieldDefinition.displayOrder)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('entity.validation.displayOrderUnique'),
          path: ['fieldDefinitions', index, 'displayOrder'],
        })
      }

      fieldKeys.add(normalizedFieldKey)
      displayOrders.add(fieldDefinition.displayOrder)
    })
  })

export const entityTypeFormSchema = createEntityTypeFormSchema()

export type EntityTypeFormValues = z.infer<typeof entityTypeFormSchema>
