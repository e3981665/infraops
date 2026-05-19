import { createContext } from 'react'
import type { SupportedLocale, TranslationKey } from '@/shared/i18n/translations'

export interface I18nContextValue {
  locale: SupportedLocale
  setLocale: (locale: SupportedLocale) => void
  supportedLocales: SupportedLocale[]
  t: (key: TranslationKey, values?: Record<string, string | number>) => string
}

export const I18nContext = createContext<I18nContextValue | null>(null)
