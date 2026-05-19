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

describe('InventoryRouteProtection', () => {
  it('should block users who do not have inventory read access', async () => {
    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-04-02T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-04-09T12:00:00Z',
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: '91f2b53f-d845-42fe-8b89-d290cf2d713f',
          fullName: 'InfraOps Viewer',
          email: 'viewer@infraops.local',
          roles: ['Viewer'],
          permissions: [],
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
          path={routePaths.inventory}
          element={
            <ProtectedRoute requiredPermission={permissionCodes.inventoryRead}>
              <div>Inventory area</div>
            </ProtectedRoute>
          }
        />
      </Routes>,
      { route: routePaths.inventory },
    )

    await waitFor(() => {
      expect(screen.getByText('Permission required.')).toBeInTheDocument()
    })

    expect(
      screen.getByText((content) => content.includes('Your current access does not include')),
    ).toBeInTheDocument()
    expect(screen.queryByText('Inventory area')).not.toBeInTheDocument()
  })
})
