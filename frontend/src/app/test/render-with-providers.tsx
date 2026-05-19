import type { ReactElement } from 'react'
import { render } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { AuthSessionProvider } from '@/modules/auth/context/AuthSessionContext'

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

  return render(
    <QueryClientProvider client={queryClient}>
      <AuthSessionProvider>
        <MemoryRouter initialEntries={[options?.route ?? '/']}>{ui}</MemoryRouter>
      </AuthSessionProvider>
    </QueryClientProvider>,
  )
}
