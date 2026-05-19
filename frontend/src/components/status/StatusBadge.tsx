import { useTranslation } from '@/shared/i18n/useTranslation'
import type { TranslationKey } from '@/shared/i18n/translations'

interface StatusBadgeProps {
  value: string
}

export function StatusBadge({ value }: StatusBadgeProps) {
  const { t } = useTranslation()
  const normalized = value.trim()
  const variant = resolveVariant(normalized)

  return <span className={`status-chip status-chip--${variant}`}>{formatStatus(normalized, t)}</span>
}

function resolveVariant(value: string) {
  if (['active', 'approved', 'operational', 'submitted'].includes(value)) {
    return 'success'
  }

  if (['rejected', 'inactive'].includes(value)) {
    return 'danger'
  }

  if (['reworkRequested', 'warning', 'draft'].includes(value)) {
    return 'warning'
  }

  return 'neutral'
}

function formatStatus(
  value: string,
  t: (key: TranslationKey, values?: Record<string, string | number>) => string,
) {
  const translationKey = `status.${value}` as TranslationKey

  if (knownStatusKeys.includes(translationKey)) {
    return t(translationKey)
  }

  return value
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (first) => first.toUpperCase())
}

const knownStatusKeys: TranslationKey[] = [
  'status.active',
  'status.inactive',
  'status.operational',
  'status.draft',
  'status.submitted',
  'status.approved',
  'status.rejected',
  'status.reworkRequested',
  'status.approve',
  'status.reject',
  'status.requestRework',
]
