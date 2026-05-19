import { screen, waitFor } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { ProtectedLayout } from '@/app/layouts/ProtectedLayout'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { routePaths } from '@/shared/routing/route-paths'

describe('AppShell', () => {
  it('should render the authenticated shell after restoring the current user', async () => {
    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-04-01T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-04-08T12:00:00Z',
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: '2f8f3b61-6c1d-4dc7-bb55-3bd49773b5a7',
          fullName: 'InfraOps Administrator',
          email: 'admin@infraops.local',
          roles: ['Admin'],
          permissions: ['inventory.read', 'preventive.validate'],
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
        <Route path={routePaths.login} element={<div>Login page</div>} />
        <Route path={routePaths.app} element={<ProtectedLayout />}>
          <Route index element={<div>Dashboard content</div>} />
        </Route>
      </Routes>,
      { route: routePaths.app },
    )

    await waitFor(() => {
      expect(screen.getByText('InfraOps Administrator')).toBeInTheDocument()
    })

    expect(screen.getByText('Dashboard content')).toBeInTheDocument()
    expect(screen.getByText('Admin')).toBeInTheDocument()
  })

  it('should activate dashboard only on the exact dashboard route', async () => {
    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-04-01T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-04-08T12:00:00Z',
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: '2f8f3b61-6c1d-4dc7-bb55-3bd49773b5a7',
          fullName: 'InfraOps Administrator',
          email: 'admin@infraops.local',
          roles: ['Admin'],
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
        <Route path={routePaths.login} element={<div>Login page</div>} />
        <Route path={routePaths.app} element={<ProtectedLayout />}>
          <Route index element={<div>Dashboard content</div>} />
          <Route path="inventory" element={<div>Inventory content</div>} />
        </Route>
      </Routes>,
      { route: routePaths.inventory },
    )

    expect(await screen.findByText('Inventory content')).toBeInTheDocument()

    expect(screen.getByRole('link', { name: /dashboard/i })).not.toHaveClass('active')
    expect(screen.getByRole('link', { name: /inventory/i })).toHaveClass('active')
  })
})
