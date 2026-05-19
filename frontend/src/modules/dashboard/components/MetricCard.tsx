interface MetricCardProps {
  label: string
  value: number
  detail?: string
}

export function MetricCard({ detail, label, value }: MetricCardProps) {
  return (
    <article className="metric-card">
      <span>{label}</span>
      <strong>{value.toLocaleString()}</strong>
      {detail ? <small>{detail}</small> : null}
    </article>
  )
}
