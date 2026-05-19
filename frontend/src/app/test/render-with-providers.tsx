import type { PropsWithChildren, ReactElement } from 'react'
import { render } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { AuthSessionProvider } from '@/modules/auth/context/AuthSessionContext'
import { I18nProvider } from '@/shared/i18n/I18nProvider'
import { ThemeProvider } from '@/shared/theme/ThemeProvider'

interface RenderWithProvidersOptions {
  route?: string
}

export function renderWithProviders(
  ui: ReactElement,
  options?: RenderWithProvidersOptions,
) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  function Providers({ children }: PropsWithChildren) {
    return (
      <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <I18nProvider>
          <AuthSessionProvider>
            <MemoryRouter initialEntries={[options?.route ?? '/']}>{children}</MemoryRouter>
          </AuthSessionProvider>
        </I18nProvider>
      </ThemeProvider>
    </QueryClientProvider>
    )
  }

  return render(ui, { wrapper: Providers })
}
