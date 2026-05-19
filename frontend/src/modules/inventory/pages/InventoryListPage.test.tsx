import { screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { InventoryListPage } from '@/modules/inventory/pages/InventoryListPage'

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('InventoryListPage', () => {
  it('should render inventory items returned by the API', async () => {
    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-04-02T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-04-09T12:00:00Z',
    })

    vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
      const url = typeof input === 'string' ? input : input instanceof URL ? input.toString() : input.url

      if (url.endsWith('/api/auth/me')) {
        return new Response(
          JSON.stringify({
            id: '91f2b53f-d845-42fe-8b89-d290cf2d713f',
            fullName: 'InfraOps Administrator',
            email: 'admin@infraops.local',
            roles: ['Admin'],
            permissions: ['inventory.read', 'inventory.write'],
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        )
      }

      if (url.endsWith('/api/inventory/form-metadata')) {
        return new Response(
          JSON.stringify({
            entityTypes: [
              {
                id: '26043c08-0880-46d9-b7dc-5778d07d64a9',
                code: 'ups',
                name: 'UPS',
              },
            ],
            regions: [
              {
                id: '8f868090-addf-4366-9946-5b418574c115',
                code: 'north-region',
                name: 'North Region',
              },
            ],
            sites: [
              {
                id: '720c4a9a-94bf-47b8-a1cf-24f346955f7e',
                regionId: '8f868090-addf-4366-9946-5b418574c115',
                code: 'north-hub',
                name: 'North Hub',
              },
            ],
            statuses: [
              {
                code: 'operational',
                label: 'Operational',
              },
            ],
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        )
      }

      if (url.endsWith('/api/inventory')) {
        return new Response(
          JSON.stringify([
            {
              id: 'f916d166-c271-4d1c-8654-ea6736648d63',
              entityTypeId: '26043c08-0880-46d9-b7dc-5778d07d64a9',
              entityTypeName: 'UPS',
              regionId: '8f868090-addf-4366-9946-5b418574c115',
              regionName: 'North Region',
              siteId: '720c4a9a-94bf-47b8-a1cf-24f346955f7e',
              siteName: 'North Hub',
              displayName: 'UPS Room A',
              status: 'operational',
              installationDate: '2024-04-01',
              isActive: true,
            },
          ]),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        )
      }

      return new Response(null, { status: 404 })
    })

    renderWithProviders(<InventoryListPage />, { route: '/app/inventory' })

    await waitFor(() => {
      expect(screen.getByText('UPS Room A')).toBeInTheDocument()
    })

    expect(screen.getByRole('cell', { name: 'North Region' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: 'North Hub' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: 'Operational' })).toBeInTheDocument()
  })
})
