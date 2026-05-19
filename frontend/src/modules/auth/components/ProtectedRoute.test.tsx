import { screen, waitFor } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { routePaths } from '@/shared/routing/route-paths'

describe('ProtectedRoute', () => {
  it('should redirect to login when the user is unauthenticated', async () => {
    renderWithProviders(
      <Routes>
        <Route path={routePaths.login} element={<div>Login page</div>} />
        <Route
          path={routePaths.app}
          element={
            <ProtectedRoute>
              <div>Protected area</div>
            </ProtectedRoute>
          }
        />
      </Routes>,
      { route: routePaths.app },
    )

    await waitFor(() => {
      expect(screen.getByText('Login page')).toBeInTheDocument()
    })
  })
})
