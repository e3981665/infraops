import { z } from 'zod'

const checklistItemTypes = ['yesNo', 'text', 'numeric', 'select'] as const

const optionSchema = z.object({
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
    .int('Option display order must be a whole number.')
    .min(1, 'Option display order must be greater than zero.'),
})

const checklistItemSchema = z
  .object({
    id: z.string().uuid().optional(),
    itemKey: z
      .string()
      .trim()
      .min(1, 'Checklist item key is required.')
      .max(80, 'Checklist item key cannot exceed 80 characters.')
      .regex(
        /^[a-z][A-Za-z0-9]*$/,
        'Checklist item key must start with a lowercase letter and use only letters and numbers.',
      ),
    label: z
      .string()
      .trim()
      .min(1, 'Checklist item label is required.')
      .max(160, 'Checklist item label cannot exceed 160 characters.'),
    itemType: z.enum(checklistItemTypes, {
      message: 'Checklist item type is required.',
    }),
    displayOrder: z.coerce
      .number()
      .int('Checklist item display order must be a whole number.')
      .min(1, 'Checklist item display order must be greater than zero.'),
    isRequired: z.boolean(),
    isActive: z.boolean(),
    helpText: z.string().max(500, 'Help text cannot exceed 500 characters.'),
    isCritical: z.boolean(),
    requiresCommentOnFailure: z.boolean(),
    requiresPhotoOnFailure: z.boolean(),
    minimumValue: z.string(),
    maximumValue: z.string(),
    options: z.array(optionSchema),
  })
  .superRefine((values, context) => {
    if (values.itemType === 'select') {
      if (values.options.length === 0) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Select checklist items must define at least one option.',
          path: ['options'],
        })
      }
    } else if (values.options.length > 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Only select checklist items can define options.',
        path: ['options'],
      })
    }

    const normalizedOptionValues = values.options.map((option) => option.value.trim().toLowerCase())
    if (normalizedOptionValues.length !== new Set(normalizedOptionValues).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Option values must be unique.',
        path: ['options'],
      })
    }

    const optionDisplayOrders = values.options.map((option) => option.displayOrder)
    if (optionDisplayOrders.length !== new Set(optionDisplayOrders).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Option display orders must be unique.',
        path: ['options'],
      })
    }

    if (values.itemType === 'numeric') {
      if (values.minimumValue && Number.isNaN(Number(values.minimumValue))) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Minimum value must be numeric.',
          path: ['minimumValue'],
        })
      }

      if (values.maximumValue && Number.isNaN(Number(values.maximumValue))) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Maximum value must be numeric.',
          path: ['maximumValue'],
        })
      }

      if (values.minimumValue && values.maximumValue && Number(values.minimumValue) > Number(values.maximumValue)) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Minimum value cannot be greater than maximum value.',
          path: ['maximumValue'],
        })
      }
    } else if (values.minimumValue.trim() || values.maximumValue.trim()) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Only numeric checklist items can define minimum or maximum values.',
        path: ['minimumValue'],
      })
    }
  })

const sectionSchema = z
  .object({
    id: z.string().uuid().optional(),
    title: z
      .string()
      .trim()
      .min(1, 'Section title is required.')
      .max(120, 'Section title cannot exceed 120 characters.'),
    displayOrder: z.coerce
      .number()
      .int('Section display order must be a whole number.')
      .min(1, 'Section display order must be greater than zero.'),
    isActive: z.boolean(),
    checklistItems: z.array(checklistItemSchema),
  })
  .superRefine((values, context) => {
    const itemDisplayOrders = values.checklistItems.map((item) => item.displayOrder)
    if (itemDisplayOrders.length !== new Set(itemDisplayOrders).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Checklist item display orders must be unique within a section.',
        path: ['checklistItems'],
      })
    }
  })

export const preventiveTemplateFormSchema = z
  .object({
    entityTypeId: z.string().uuid('Entity type is required.'),
    name: z
      .string()
      .trim()
      .min(1, 'Template name is required.')
      .max(120, 'Template name cannot exceed 120 characters.'),
    code: z
      .string()
      .trim()
      .min(1, 'Template code is required.')
      .max(60, 'Template code cannot exceed 60 characters.')
      .regex(
        /^[a-z0-9]+(?:-[a-z0-9]+)*$/,
        'Template code must use lowercase letters, numbers, and hyphens only.',
      ),
    description: z.string().max(500, 'Description cannot exceed 500 characters.'),
    sections: z.array(sectionSchema),
  })
  .superRefine((values, context) => {
    const sectionDisplayOrders = values.sections.map((section) => section.displayOrder)
    if (sectionDisplayOrders.length !== new Set(sectionDisplayOrders).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Section display orders must be unique.',
        path: ['sections'],
      })
    }

    const normalizedItemKeys = values.sections.flatMap((section) =>
      section.checklistItems.map((item) => item.itemKey.trim().toLowerCase()),
    )

    if (normalizedItemKeys.length !== new Set(normalizedItemKeys).size) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Checklist item keys must be unique within a template.',
        path: ['sections'],
      })
    }
  })

export type PreventiveTemplateFormValues = z.infer<typeof preventiveTemplateFormSchema>
