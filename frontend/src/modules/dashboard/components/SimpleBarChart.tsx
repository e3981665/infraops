import type { DashboardMetricPoint } from '@/modules/dashboard/types/dashboard'

interface SimpleBarChartProps {
  title: string
  points: DashboardMetricPoint[]
  emptyMessage: string
}

export function SimpleBarChart({ emptyMessage, points, title }: SimpleBarChartProps) {
  const maxValue = Math.max(...points.map((point) => point.value), 0)

  return (
    <article className="chart-panel">
      <h2>{title}</h2>
      {points.length === 0 || maxValue === 0 ? (
        <p className="empty-copy">{emptyMessage}</p>
      ) : (
        <div className="bar-chart" role="img" aria-label={title}>
          {points.map((point) => (
            <div className="bar-chart__row" key={point.label}>
              <span>{point.label}</span>
              <div className="bar-chart__track">
                <div
                  className="bar-chart__bar"
                  style={{ width: `${Math.max((point.value / maxValue) * 100, 4)}%` }}
                />
              </div>
              <strong>{point.value}</strong>
            </div>
          ))}
        </div>
      )}
    </article>
  )
}
