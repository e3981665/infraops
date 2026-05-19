import { startTransition } from 'react'
import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/http-client'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { useTranslation } from '@/shared/i18n/useTranslation'
import {
  loginFormSchema,
  type LoginFormValues,
} from '@/modules/auth/schemas/login-form-schema'
import { routePaths } from '@/shared/routing/route-paths'

interface NavigationState {
  from?: string
}

export function LoginPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { isAuthenticated, signIn, status } = useAuthSession()
  const { t } = useTranslation()
  const [submitError, setSubmitError] = useState<string | null>(null)

  const redirectTarget =
    (location.state as NavigationState | null)?.from || routePaths.app

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginFormSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  })

  if (status === 'loading') {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('login.sessionBootstrap')}</p>
        <h1>{t('login.checkingTitle')}</h1>
        <p>{t('login.checkingMessage')}</p>
      </section>
    )
  }

  if (isAuthenticated) {
    return <Navigate to={routePaths.app} replace />
  }

  async function onSubmit(values: LoginFormValues) {
    setSubmitError(null)

    try {
      await signIn(values)
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        setSubmitError(t('login.invalidCredentials'))
        return
      }

      setSubmitError(t('login.failed'))
      return
    }

    startTransition(() => {
      navigate(redirectTarget, { replace: true })
    })
  }

  return (
    <section className="login-page">
      <aside className="login-page__summary" aria-label={t('login.platformSummary')}>
        <p className="hero-panel__eyebrow">{t('login.summaryEyebrow')}</p>
        <h1>{t('login.summaryTitle')}</h1>
        <ul>
          <li>{t('login.summaryInventory')}</li>
          <li>{t('login.summarySnapshots')}</li>
          <li>{t('login.summaryAudit')}</li>
        </ul>
      </aside>
      <div className="login-panel">
        <p className="hero-panel__eyebrow">{t('login.auth')}</p>
        <h1>{t('login.title')}</h1>
        <p>{t('login.description')}</p>

        <form className="form-grid" onSubmit={handleSubmit(onSubmit)} noValidate>
          <div className="field">
            <label htmlFor="email">{t('login.email')}</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              placeholder={t('login.emailPlaceholder')}
              {...register('email')}
            />
            {errors.email ? (
              <span className="field__error">{errors.email.message}</span>
            ) : null}
          </div>

          <div className="field">
            <label htmlFor="password">{t('login.password')}</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              placeholder={t('login.passwordPlaceholder')}
              {...register('password')}
            />
            {errors.password ? (
              <span className="field__error">{errors.password.message}</span>
            ) : null}
          </div>

          <button className="button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? t('login.signingIn') : t('login.signIn')}
          </button>
        </form>

        {submitError ? <p className="form-error">{submitError}</p> : null}

        <p className="form-note">
          {t('login.devUser')} <strong>admin@infraops.local</strong>
        </p>
      </div>
    </section>
  )
}
