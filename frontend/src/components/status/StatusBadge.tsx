interface StatusBadgeProps {
  value: string
}

export function StatusBadge({ value }: StatusBadgeProps) {
  const normalized = value.trim()
  const variant = resolveVariant(normalized)

  return <span className={`status-chip status-chip--${variant}`}>{formatStatus(normalized)}</span>
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

function formatStatus(value: string) {
  return value
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (first) => first.toUpperCase())
}
