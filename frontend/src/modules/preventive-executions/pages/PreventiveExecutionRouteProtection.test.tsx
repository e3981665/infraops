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

describe('PreventiveExecutionRouteProtection', () => {
  it('should block users who do not have preventive execution read access', async () => {
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
          path={routePaths.preventiveExecutions}
          element={
            <ProtectedRoute requiredPermission={permissionCodes.preventiveRead}>
              <div>Preventive execution area</div>
            </ProtectedRoute>
          }
        />
      </Routes>,
      { route: routePaths.preventiveExecutions },
    )

    await waitFor(() => {
      expect(screen.getByText('Permission required.')).toBeInTheDocument()
    })

    expect(screen.queryByText('Preventive execution area')).not.toBeInTheDocument()
  })
})
