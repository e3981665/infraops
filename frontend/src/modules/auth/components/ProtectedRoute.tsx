import type { PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { useTranslation } from '@/shared/i18n/useTranslation'
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
  const { t } = useTranslation()

  if (status === 'loading') {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('auth.sessionBootstrap')}</p>
        <h1>{t('auth.restoringAccess')}</h1>
        <p>{t('auth.restoringAccessMessage')}</p>
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
        <p className="hero-panel__eyebrow">{t('auth.accessDenied')}</p>
        <h1>{t('auth.permissionRequired')}</h1>
        <p>
          {t('auth.missingPermission', { permission: requiredPermission })}
        </p>
      </section>
    )
  }

  return <>{children}</>
}
