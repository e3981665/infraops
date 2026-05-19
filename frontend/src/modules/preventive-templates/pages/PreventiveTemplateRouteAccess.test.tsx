import { screen, waitFor } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { routePaths } from '@/shared/routing/route-paths'

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('Preventive template admin route', () => {
  it('should block authenticated users who do not have preventive template read permission', async () => {
    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-04-02T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-04-09T12:00:00Z',
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'c6ee8fea-d1e9-49a7-8a11-43c04c8d38f2',
          fullName: 'Technician User',
          email: 'technician@infraops.local',
          roles: ['Technician'],
          permissions: ['inventory.read'],
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      ),
    )

    renderWithProviders(
      <Routes>
        <Route
          path={routePaths.preventiveTemplates}
          element={
            <ProtectedRoute requiredPermission={permissionCodes.preventiveTemplatesRead}>
              <div>Preventive template area</div>
            </ProtectedRoute>
          }
        />
      </Routes>,
      { route: routePaths.preventiveTemplates },
    )

    await waitFor(() => {
      expect(screen.getByText('Permission required.')).toBeInTheDocument()
    })

    expect(screen.queryByText('Preventive template area')).not.toBeInTheDocument()
  })
})
