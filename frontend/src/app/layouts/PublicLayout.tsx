import { Link, Outlet } from 'react-router-dom'
import { routePaths } from '@/shared/routing/route-paths'

export function PublicLayout() {
  return (
    <div className="public-layout">
      <header className="public-layout__header">
        <Link className="brand" to={routePaths.home}>
          InfraOps
        </Link>
        <nav className="public-layout__nav" aria-label="Public navigation">
          <Link to={routePaths.home}>Overview</Link>
          <Link to={routePaths.login}>Login</Link>
        </nav>
      </header>
      <main className="public-layout__content">
        <Outlet />
      </main>
    </div>
  )
}
