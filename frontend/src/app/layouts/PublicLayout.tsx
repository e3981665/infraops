import { Link, Outlet } from 'react-router-dom'
import { routePaths } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function PublicLayout() {
  const { t } = useTranslation()

  return (
    <div className="public-layout">
      <header className="public-layout__header">
        <Link className="brand" to={routePaths.home}>
          {t('app.brand')}
        </Link>
        <nav className="public-layout__nav" aria-label={t('app.publicNavigation')}>
          <Link to={routePaths.home}>{t('app.publicOverview')}</Link>
          <Link to={routePaths.login}>{t('app.login')}</Link>
        </nav>
      </header>
      <main className="public-layout__content">
        <Outlet />
      </main>
    </div>
  )
}
