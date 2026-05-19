import { startTransition } from 'react'
import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/http-client'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
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
        <p className="hero-panel__eyebrow">Session bootstrap</p>
        <h1>Checking saved credentials.</h1>
        <p>The app is restoring the current InfraOps session.</p>
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
        setSubmitError('The email or password is invalid.')
        return
      }

      setSubmitError('InfraOps could not complete the sign-in request.')
      return
    }

    startTransition(() => {
      navigate(redirectTarget, { replace: true })
    })
  }

  return (
    <section className="login-page">
      <div className="login-panel">
        <p className="hero-panel__eyebrow">Authentication</p>
        <h1>Sign in to InfraOps</h1>
        <p>
          Use the seeded development administrator account to enter the
          authenticated workspace. The frontend restores the current user on app
          load and refreshes tokens when possible.
        </p>

        <form className="form-grid" onSubmit={handleSubmit(onSubmit)} noValidate>
          <div className="field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              placeholder="operator@infraops.local"
              {...register('email')}
            />
            {errors.email ? (
              <span className="field__error">{errors.email.message}</span>
            ) : null}
          </div>

          <div className="field">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              placeholder="At least 8 characters"
              {...register('password')}
            />
            {errors.password ? (
              <span className="field__error">{errors.password.message}</span>
            ) : null}
          </div>

          <button className="button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        {submitError ? <p className="form-error">{submitError}</p> : null}

        <p className="form-note">
          Development user: <strong>admin@infraops.local</strong>
        </p>
      </div>
    </section>
  )
}
