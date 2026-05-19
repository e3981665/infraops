import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { AppErrorBoundary } from '@/app/components/AppErrorBoundary'
import { I18nProvider } from '@/shared/i18n/I18nProvider'

function ThrowingRoute() {
  throw new Error('Route failed')
  return null
}

describe('AppErrorBoundary', () => {
  it('should render the route fallback when a child route fails', () => {
    vi.spyOn(console, 'error').mockImplementation(() => undefined)

    render(
      <I18nProvider>
        <AppErrorBoundary>
          <ThrowingRoute />
        </AppErrorBoundary>
      </I18nProvider>,
    )

    expect(
      screen.getByRole('heading', { name: 'This workspace view could not be loaded.' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Reload view' })).toBeInTheDocument()
  })
})
