import { screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { PreventiveTemplateListPage } from '@/modules/preventive-templates/pages/PreventiveTemplateListPage'

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('PreventiveTemplateListPage', () => {
  it('should render preventive templates returned by the API', async () => {
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
            permissions: ['preventive.templates.read', 'preventive.templates.write'],
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        )
      }

      if (url.endsWith('/api/preventive-templates/form-metadata')) {
        return new Response(
          JSON.stringify({
            entityTypes: [
              {
                id: '26043c08-0880-46d9-b7dc-5778d07d64a9',
                code: 'ups',
                name: 'UPS',
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

      if (url.endsWith('/api/preventive-templates')) {
        return new Response(
          JSON.stringify([
            {
              id: 'f916d166-c271-4d1c-8654-ea6736648d63',
              entityTypeId: '26043c08-0880-46d9-b7dc-5778d07d64a9',
              entityTypeName: 'UPS',
              name: 'UPS Quarterly Inspection',
              code: 'ups-quarterly-inspection',
              description: 'Quarterly checklist.',
              isActive: true,
              sectionCount: 2,
              checklistItemCount: 5,
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

    renderWithProviders(<PreventiveTemplateListPage />, { route: '/app/preventive-templates' })

    await waitFor(() => {
      expect(screen.getByText('UPS Quarterly Inspection')).toBeInTheDocument()
    })

    expect(screen.getByText('Quarterly checklist.')).toBeInTheDocument()
    expect(screen.getByText('ups-quarterly-inspection')).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: 'UPS' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: '2' })).toBeInTheDocument()
    expect(screen.getByRole('cell', { name: '5' })).toBeInTheDocument()
  })
})
