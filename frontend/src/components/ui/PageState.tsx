import type { ReactNode } from 'react'

interface PageStateProps {
  action?: ReactNode
  className?: string
  eyebrow?: string
  message?: ReactNode
  title: string
}

export function PageState({
  action,
  className = '',
  eyebrow = 'InfraOps',
  message,
  title,
}: PageStateProps) {
  return (
    <section className={`status-panel ${className}`.trim()}>
      <p className="hero-panel__eyebrow">{eyebrow}</p>
      <h1>{title}</h1>
      {message ? <p>{message}</p> : null}
      {action ? <div className="button-row">{action}</div> : null}
    </section>
  )
}
