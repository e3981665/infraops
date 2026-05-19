import { screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { EntityTypeListPage } from '@/modules/entity-types/pages/EntityTypeListPage'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('EntityTypeListPage', () => {
  it('should render entity types returned by the API', async () => {
    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-04-01T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-04-08T12:00:00Z',
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
            permissions: ['entity.manage'],
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        )
      }

      if (url.endsWith('/api/entity-types')) {
        return new Response(
          JSON.stringify([
            {
              id: '0d2c5041-dbd1-445c-b72e-3a682cf83b6f',
              name: 'UPS',
              code: 'ups',
              description: 'Critical power backup assets.',
              isActive: true,
              fieldCount: 2,
            },
            {
              id: 'bf99774f-d945-402d-af51-53213e597339',
              name: 'Generator',
              code: 'generator',
              description: 'Backup generation assets.',
              isActive: false,
              fieldCount: 4,
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

    renderWithProviders(<EntityTypeListPage />, { route: '/app/entity-types' })

    await waitFor(() => {
      expect(screen.getByText('UPS')).toBeInTheDocument()
    })

    expect(screen.getByText('Critical power backup assets.')).toBeInTheDocument()
    expect(screen.getByText('generator')).toBeInTheDocument()
    expect(screen.getByText('4')).toBeInTheDocument()
  })
})
