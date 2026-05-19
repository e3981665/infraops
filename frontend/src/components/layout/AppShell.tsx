import type { PropsWithChildren } from 'react'
import { NavLink } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { protectedNavigationItems } from '@/shared/routing/protected-navigation'
import { routePaths } from '@/shared/routing/route-paths'
import { useTheme } from '@/shared/theme/useTheme'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

export function AppShell({ children }: PropsWithChildren) {
  const { hasPermission, session, signOut } = useAuthSession()
  const { locale, setLocale, supportedLocales, t } = useTranslation()
  const { theme, toggleTheme } = useTheme()

  const visibleNavigationItems = protectedNavigationItems.filter(
    (item) => !item.requiredPermission || hasPermission(item.requiredPermission),
  )

  return (
    <div className="app-shell">
      <header className="app-shell__header">
        <div className="app-shell__identity">
          <NavLink className="brand" to={routePaths.app}>
            {t('app.brand')}
          </NavLink>
          <div className="app-shell__topbar-title">
            <span>{t('app.console')}</span>
          </div>
          <div className="app-shell__meta">
            {session ? (
              <>
                <strong>{localizeDemoText(session.currentUser.fullName, locale)}</strong>
                <span>{session.currentUser.email}</span>
              </>
            ) : (
              t('app.notSignedIn')
            )}
          </div>
        </div>
        <div className="app-shell__actions">
          <button
            aria-label={theme === 'dark' ? t('app.theme.switchToLight') : t('app.theme.switchToDark')}
            className="app-shell__control app-shell__theme-toggle"
            type="button"
            onClick={toggleTheme}
          >
            <span className="app-shell__theme-mark" aria-hidden="true" />
            <span>{theme === 'dark' ? t('app.theme.dark') : t('app.theme.light')}</span>
          </button>
          <label className="app-shell__control app-shell__language" htmlFor="appLanguage">
            <span className="sr-only">{t('app.language')}</span>
            <select
              id="appLanguage"
              aria-label={t('app.language')}
              value={locale}
              onChange={(event) => setLocale(event.target.value as typeof locale)}
            >
              {supportedLocales.map((supportedLocale) => (
                <option key={supportedLocale} value={supportedLocale}>
                  {supportedLocale === 'en-US' ? 'EN' : 'PT-BR'}
                </option>
              ))}
            </select>
          </label>
          {session ? (
            <div className="role-badges" aria-label={t('app.assignedRoles')}>
              {session.currentUser.roles.map((role) => (
                <span className="role-badge" key={role}>
                  {localizeDemoText(role, locale)}
                </span>
              ))}
            </div>
          ) : null}
          <button
            className="app-shell__control app-shell__sign-out"
            type="button"
            onClick={() => void signOut()}
          >
            {t('app.signOut')}
          </button>
        </div>
      </header>
      <div className="app-shell__body">
        <aside className="app-shell__sidebar">
          <NavLink className="app-shell__sidebar-brand" to={routePaths.app}>
            <span>IO</span>
            <strong>{t('app.brand')}</strong>
          </NavLink>
          <p className="hero-panel__eyebrow">{t('app.workspace')}</p>
          <nav className="app-shell__nav" aria-label={t('app.applicationNavigation')}>
            {visibleNavigationItems.map((item) => (
              <NavLink key={item.to} to={item.to} end={item.to === routePaths.app}>
                <span className="app-shell__nav-marker" aria-hidden="true" />
                <span>{t(item.labelKey)}</span>
                <small>{t(item.descriptionKey)}</small>
              </NavLink>
            ))}
          </nav>
          <NavLink className="app-shell__overview-link" to={routePaths.home}>
            {t('app.overview')}
          </NavLink>
        </aside>
        <main className="app-shell__main">
          <div className="app-shell__content">{children}</div>
        </main>
      </div>
    </div>
  )
}
