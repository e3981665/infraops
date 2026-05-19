import { useContext } from 'react'
import { I18nContext } from '@/shared/i18n/i18n-context'

export function useTranslation() {
  const context = useContext(I18nContext)

  if (!context) {
    throw new Error('useTranslation must be used inside I18nProvider.')
  }

  return context
}
