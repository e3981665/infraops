import { z } from 'zod'
import {
  defaultSchemaTranslate,
  type SchemaTranslate,
} from '@/shared/i18n/schema-translation'

export const createLoginFormSchema = (t: SchemaTranslate = defaultSchemaTranslate) => z.object({
  email: z.string().email(t('login.validation.email')),
  password: z
    .string()
    .min(8, t('login.validation.passwordMin')),
})

export const loginFormSchema = createLoginFormSchema()

export type LoginFormValues = z.infer<typeof loginFormSchema>
