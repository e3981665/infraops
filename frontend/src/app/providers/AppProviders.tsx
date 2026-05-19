import type { PropsWithChildren } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from '@/app/providers/query-client'
import { AuthSessionProvider } from '@/modules/auth/context/AuthSessionContext'
import { I18nProvider } from '@/shared/i18n/I18nProvider'
import { ThemeProvider } from '@/shared/theme/ThemeProvider'

export function AppProviders({ children }: PropsWithChildren) {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <I18nProvider>
          <AuthSessionProvider>{children}</AuthSessionProvider>
        </I18nProvider>
      </ThemeProvider>
    </QueryClientProvider>
  )
}
