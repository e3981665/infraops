import { Link } from 'react-router-dom'
import { environment } from '@/shared/config/environment'
import { routePaths } from '@/shared/routing/route-paths'

const capabilityCards = [
  {
    title: 'Configurable Entity Types',
    description:
      'Prepare dynamic field definitions, inventory metadata, and portfolio-safe domain language from the start.',
  },
  {
    title: 'Preventive Maintenance',
    description:
      'Support templates, checklists, execution workflows, and validation without hard-coding every equipment flow.',
  },
  {
    title: 'Enterprise Foundations',
    description:
      'Keep authentication, authorization, regions, sites, audit metadata, and testing boundaries explicit.',
  },
] as const

export function HomePage() {
  return (
    <>
      <section className="hero-panel">
        <div>
          <p className="hero-panel__eyebrow">Authentication foundation</p>
          <h1>Infrastructure operations, prepared for clean growth.</h1>
        </div>
        <p>
          InfraOps now starts with JWT-backed authentication, refresh tokens,
          seeded roles and permissions, protected API endpoints, and a React
          shell that restores the current session on load.
        </p>
        <div className="button-row">
          <Link className="button" to={routePaths.login}>
            Open Login
          </Link>
          <Link className="button--secondary" to={routePaths.app}>
            Open Protected Area
          </Link>
        </div>
        <p className="form-note">Configured API base URL: {environment.apiBaseUrl}</p>
      </section>

      <section className="card-grid" aria-label="Platform capabilities">
        {capabilityCards.map((card) => (
          <article className="card" key={card.title}>
            <h2>{card.title}</h2>
            <p>{card.description}</p>
          </article>
        ))}
      </section>
    </>
  )
}
