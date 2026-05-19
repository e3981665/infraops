import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { RouteLoadingFallback } from '@/app/components/RouteLoadingFallback'
import { I18nProvider } from '@/shared/i18n/I18nProvider'

describe('RouteLoadingFallback', () => {
  it('should render a stable loading state for lazy route chunks', () => {
    render(
      <I18nProvider>
        <RouteLoadingFallback />
      </I18nProvider>,
    )

    expect(screen.getByRole('heading', { name: 'Loading workspace view.' })).toBeInTheDocument()
  })
})
