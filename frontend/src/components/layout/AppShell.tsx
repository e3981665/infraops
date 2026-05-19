import type { PropsWithChildren } from 'react'
import { NavLink } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { protectedNavigationItems } from '@/shared/routing/protected-navigation'
import { routePaths } from '@/shared/routing/route-paths'

export function AppShell({ children }: PropsWithChildren) {
  const { hasPermission, session, signOut } = useAuthSession()

  const visibleNavigationItems = protectedNavigationItems.filter(
    (item) => !item.requiredPermission || hasPermission(item.requiredPermission),
  )

  return (
    <div className="app-shell">
      <header className="app-shell__header">
        <div className="app-shell__identity">
          <NavLink className="brand" to={routePaths.app}>
            InfraOps
          </NavLink>
          <div className="app-shell__meta">
            {session ? (
              <>
                <strong>{session.currentUser.fullName}</strong>
                <span>{session.currentUser.email}</span>
              </>
            ) : (
              'Not signed in'
            )}
          </div>
        </div>
        <div className="app-shell__actions">
          {session ? (
            <div className="role-badges" aria-label="Assigned roles">
              {session.currentUser.roles.map((role) => (
                <span className="role-badge" key={role}>
                  {role}
                </span>
              ))}
            </div>
          ) : null}
          <button className="button--secondary" type="button" onClick={() => void signOut()}>
            Sign out
          </button>
        </div>
      </header>
      <div className="app-shell__body">
        <aside className="app-shell__sidebar">
          <p className="hero-panel__eyebrow">Workspace</p>
          <nav className="app-shell__nav" aria-label="Application navigation">
            {visibleNavigationItems.map((item) => (
              <NavLink key={item.to} to={item.to}>
                <span>{item.label}</span>
                <small>{item.description}</small>
              </NavLink>
            ))}
          </nav>
          <NavLink className="app-shell__overview-link" to={routePaths.home}>
            Return to overview
          </NavLink>
        </aside>
        <main className="app-shell__main">
          <div className="app-shell__content">{children}</div>
        </main>
      </div>
    </div>
  )
}
