import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react'
import { I18nContext, type I18nContextValue } from '@/shared/i18n/i18n-context'
import {
  translations,
  type SupportedLocale,
  type TranslationKey,
} from '@/shared/i18n/translations'

const localeStorageKey = 'infraops.locale'
const supportedLocales = Object.keys(translations) as SupportedLocale[]

export function I18nProvider({ children }: PropsWithChildren) {
  const [locale, setLocaleState] = useState<SupportedLocale>(() => resolveInitialLocale())

  useEffect(() => {
    document.documentElement.lang = locale
    window.localStorage.setItem(localeStorageKey, locale)
  }, [locale])

  const setLocale = useCallback((nextLocale: SupportedLocale) => {
    setLocaleState(nextLocale)
  }, [])

  const t = useCallback(
    (key: TranslationKey, values?: Record<string, string | number>) => {
      const template = String(translations[locale][key] ?? translations['en-US'][key] ?? key)

      if (!values) {
        return template
      }

      return Object.entries(values).reduce<string>(
        (current, [name, value]) => current.replaceAll(`{{${name}}}`, String(value)),
        template,
      )
    },
    [locale],
  )

  const value = useMemo<I18nContextValue>(
    () => ({
      locale,
      setLocale,
      supportedLocales,
      t,
    }),
    [locale, setLocale, t],
  )

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

function resolveInitialLocale(): SupportedLocale {
  const savedLocale = window.localStorage.getItem(localeStorageKey)

  if (isSupportedLocale(savedLocale)) {
    return savedLocale
  }

  const browserLocale = navigator.language

  if (isSupportedLocale(browserLocale)) {
    return browserLocale
  }

  if (browserLocale.toLowerCase().startsWith('pt')) {
    return 'pt-BR'
  }

  return 'en-US'
}

function isSupportedLocale(value: string | null): value is SupportedLocale {
  return Boolean(value && supportedLocales.includes(value as SupportedLocale))
}
