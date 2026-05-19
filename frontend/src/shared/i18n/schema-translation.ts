import type { I18nContextValue } from '@/shared/i18n/i18n-context'

export type SchemaTranslate = I18nContextValue['t']

export const defaultSchemaTranslate: SchemaTranslate = (key, values) => {
  const fallback = String(key)

  if (!values) {
    return fallback
  }

  return Object.entries(values).reduce<string>(
    (current, [name, value]) => current.replaceAll(`{{${name}}}`, String(value)),
    fallback,
  )
}
