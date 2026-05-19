import type { PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { routePaths } from '@/shared/routing/route-paths'

interface ProtectedRouteProps extends PropsWithChildren {
  requiredPermission?: string
}

export function ProtectedRoute({
  children,
  requiredPermission,
}: ProtectedRouteProps) {
  const location = useLocation()
  const { hasPermission, isAuthenticated, status } = useAuthSession()

  if (status === 'loading') {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Session bootstrap</p>
        <h1>Restoring authenticated access.</h1>
        <p>
          InfraOps is validating the saved session and loading the protected
          workspace.
        </p>
      </section>
    )
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to={routePaths.login}
        replace
        state={{ from: location.pathname }}
      />
    )
  }

  if (requiredPermission && !hasPermission(requiredPermission)) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Access denied</p>
        <h1>Permission required.</h1>
        <p>
          Your current access does not include <strong>{requiredPermission}</strong>.
        </p>
      </section>
    )
  }

  return <>{children}</>
}
