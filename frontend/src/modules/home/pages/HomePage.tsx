import { Link } from 'react-router-dom'
import { environment } from '@/shared/config/environment'
import { routePaths } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function HomePage() {
  const { t } = useTranslation()
  const capabilityCards = [
    {
      title: t('home.entityTypesTitle'),
      description: t('home.entityTypesDescription'),
    },
    {
      title: t('home.preventiveTitle'),
      description: t('home.preventiveDescription'),
    },
    {
      title: t('home.foundationsTitle'),
      description: t('home.foundationsDescription'),
    },
  ]

  return (
    <>
      <section className="hero-panel">
        <div>
          <p className="hero-panel__eyebrow">{t('home.eyebrow')}</p>
          <h1>{t('home.title')}</h1>
        </div>
        <p>{t('home.description')}</p>
        <div className="button-row">
          <Link className="button" to={routePaths.login}>
            {t('home.openLogin')}
          </Link>
          <Link className="button--secondary" to={routePaths.app}>
            {t('home.openProtectedArea')}
          </Link>
        </div>
        <p className="form-note">{t('home.apiBaseUrl', { url: environment.apiBaseUrl })}</p>
      </section>

      <section className="card-grid" aria-label={t('home.capabilitiesLabel')}>
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
