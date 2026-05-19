import { z } from 'zod'
import {
  defaultSchemaTranslate,
  type SchemaTranslate,
} from '@/shared/i18n/schema-translation'

const checklistItemTypes = ['yesNo', 'text', 'numeric', 'select'] as const

const createOptionSchema = (t: SchemaTranslate) => z.object({
  id: z.string().uuid().optional(),
  value: z
    .string()
    .trim()
    .min(1, t('templates.validation.optionValueRequired'))
    .max(80, t('templates.validation.optionValueMax')),
  label: z
    .string()
    .trim()
    .min(1, t('templates.validation.optionLabelRequired'))
    .max(120, t('templates.validation.optionLabelMax')),
  displayOrder: z.coerce
    .number()
    .int(t('templates.validation.optionDisplayOrderWholeNumber'))
    .min(1, t('templates.validation.optionDisplayOrderPositive')),
})

const createChecklistItemSchema = (t: SchemaTranslate) => z
  .object({
    id: z.string().uuid().optional(),
    itemKey: z
      .string()
      .trim()
      .min(1, t('templates.validation.itemKeyRequired'))
      .max(80, t('templates.validation.itemKeyMax'))
      .regex(
        /^[a-z][A-Za-z0-9]*$/,
        t('templates.validation.itemKeyPattern'),
      ),
    label: z
      .string()
      .trim()
      .min(1, t('templates.validation.itemLabelRequired'))
      .max(160, t('templates.validation.itemLabelMax')),
    itemType: z.enum(checklistItemTypes, {
      message: t('templates.validation.itemTypeRequired'),
    }),
    displayOrder: z.coerce
      .number()
      .int(t('templates.validation.itemDisplayOrderWholeNumber'))
      .min(1, t('templates.validation.itemDisplayOrderPositive')),
    isRequired: z.boolean(),
    isActive: z.boolean(),
    helpText: z.string().max(500, t('templates.validation.helpTextMax')),
    isCritical: z.boolean(),
    requiresCommentOnFailure: z.boolean(),
    requiresPhotoOnFailure: z.boolean(),
    minimumValue: z.string(),
    maximumValue: z.string(),
    options: z.array(createOptionSchema(t)),
  })
  .superRefine((values, context) => {
    if (values.itemType === 'select') {
      if (values.options.length === 0) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('templates.validation.selectRequiresOption'),
          path: ['options'],
        })
      }
    } else if (values.options.length > 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.onlySelectCanDefineOptions'),
        path: ['options'],
      })
    }

    const normalizedOptionValues = values.options.map((option) => option.value.trim().toLowerCase())
    if (normalizedOptionValues.length !== new Set(normalizedOptionValues).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.optionValuesUnique'),
        path: ['options'],
      })
    }

    const optionDisplayOrders = values.options.map((option) => option.displayOrder)
    if (optionDisplayOrders.length !== new Set(optionDisplayOrders).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.optionDisplayOrdersUnique'),
        path: ['options'],
      })
    }

    if (values.itemType === 'numeric') {
      if (values.minimumValue && Number.isNaN(Number(values.minimumValue))) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('templates.validation.minimumNumeric'),
          path: ['minimumValue'],
        })
      }

      if (values.maximumValue && Number.isNaN(Number(values.maximumValue))) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('templates.validation.maximumNumeric'),
          path: ['maximumValue'],
        })
      }

      if (values.minimumValue && values.maximumValue && Number(values.minimumValue) > Number(values.maximumValue)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: t('templates.validation.minimumGreaterThanMaximum'),
          path: ['maximumValue'],
        })
      }
    } else if (values.minimumValue.trim() || values.maximumValue.trim()) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.onlyNumericCanDefineBounds'),
        path: ['minimumValue'],
      })
    }
  })

const createSectionSchema = (t: SchemaTranslate) => z
  .object({
    id: z.string().uuid().optional(),
    title: z
      .string()
      .trim()
      .min(1, t('templates.validation.sectionTitleRequired'))
      .max(120, t('templates.validation.sectionTitleMax')),
    displayOrder: z.coerce
      .number()
      .int(t('templates.validation.sectionDisplayOrderWholeNumber'))
      .min(1, t('templates.validation.sectionDisplayOrderPositive')),
    isActive: z.boolean(),
    checklistItems: z.array(createChecklistItemSchema(t)),
  })
  .superRefine((values, context) => {
    const itemDisplayOrders = values.checklistItems.map((item) => item.displayOrder)
    if (itemDisplayOrders.length !== new Set(itemDisplayOrders).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.itemDisplayOrdersUniqueWithinSection'),
        path: ['checklistItems'],
      })
    }
  })

export const createPreventiveTemplateFormSchema = (
  t: SchemaTranslate = defaultSchemaTranslate,
) => z
  .object({
    entityTypeId: z.string().uuid(t('templates.validation.entityTypeRequired')),
    name: z
      .string()
      .trim()
      .min(1, t('templates.validation.nameRequired'))
      .max(120, t('templates.validation.nameMax')),
    code: z
      .string()
      .trim()
      .min(1, t('templates.validation.codeRequired'))
      .max(60, t('templates.validation.codeMax'))
      .regex(
        /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
        t('templates.validation.codePattern'),
      ),
    description: z.string().max(500, t('templates.validation.descriptionMax')),
    sections: z.array(createSectionSchema(t)),
  })
  .superRefine((values, context) => {
    const sectionDisplayOrders = values.sections.map((section) => section.displayOrder)
    if (sectionDisplayOrders.length !== new Set(sectionDisplayOrders).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.sectionDisplayOrdersUnique'),
        path: ['sections'],
      })
    }

    const normalizedItemKeys = values.sections.flatMap((section) =>
      section.checklistItems.map((item) => item.itemKey.trim().toLowerCase()),
    )

    if (normalizedItemKeys.length !== new Set(normalizedItemKeys).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: t('templates.validation.itemKeysUniqueWithinTemplate'),
        path: ['sections'],
      })
    }
  })

export const preventiveTemplateFormSchema = createPreventiveTemplateFormSchema()

export type PreventiveTemplateFormValues = z.infer<typeof preventiveTemplateFormSchema>
