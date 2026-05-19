import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { PreventiveExecutionStartPage } from '@/modules/preventive-executions/pages/PreventiveExecutionStartPage'

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('PreventiveExecutionStartPage', () => {
  it('should submit a newly started execution with the returned execution id', async () => {
    const user = userEvent.setup()
    const submittedUrls: string[] = []

    authTokenStorage.write({
      accessToken: 'access-token',
      accessTokenExpiresAtUtc: '2026-05-19T12:15:00Z',
      refreshToken: 'refresh-token',
      refreshTokenExpiresAtUtc: '2026-05-26T12:00:00Z',
    })

    vi.spyOn(globalThis, 'fetch').mockImplementation(async (input, init) => {
      const url = typeof input === 'string' ? input : input instanceof URL ? input.toString() : input.url
      const method = init?.method ?? 'GET'

      if (url.endsWith('/api/auth/me')) {
        return jsonResponse({
          id: 'admin-1',
          fullName: 'InfraOps Administrator',
          email: 'admin@infraops.local',
          roles: ['Admin'],
          permissions: ['preventive.execute', 'preventive.read'],
        })
      }

      if (url.includes('/api/inventory')) {
        return jsonResponse([
          {
            id: 'inventory-1',
            entityTypeId: 'entity-type-1',
            entityTypeName: 'UPS',
            regionId: 'region-1',
            regionName: 'North Region',
            siteId: 'site-1',
            siteName: 'North Hub',
            displayName: 'UPS-01',
            status: 'operational',
            installationDate: '2024-04-01',
            isActive: true,
          },
        ])
      }

      if (url.endsWith('/api/preventive-executions/form-definition/inventory-1')) {
        return jsonResponse({
          inventoryItemId: 'inventory-1',
          inventoryItemDisplayName: 'UPS-01',
          entityTypeId: 'entity-type-1',
          entityTypeName: 'UPS',
          entityTypeCode: 'ups',
          preventiveTemplateId: 'template-1',
          preventiveTemplateName: 'Quarterly UPS Inspection',
          preventiveTemplateCode: 'quarterly-ups',
          sections: [
            {
              id: 'section-1',
              sourceTemplateSectionId: 'template-section-1',
              title: 'Visual Inspection',
              displayOrder: 1,
              checklistItems: [
                {
                  id: 'item-1',
                  sourceChecklistItemId: 'template-item-1',
                  itemKey: 'equipmentClean',
                  label: 'Equipment clean?',
                  itemType: 'yesNo',
                  displayOrder: 1,
                  isRequired: true,
                  helpText: null,
                  isCritical: false,
                  requiresCommentOnFailure: false,
                  requiresPhotoOnFailure: false,
                  minimumValue: null,
                  maximumValue: null,
                  options: [],
                },
              ],
            },
          ],
        })
      }

      if (url.endsWith('/api/preventive-executions/start') && method === 'POST') {
        return jsonResponse(buildExecutionDetails('execution-1', []), 201)
      }

      if (url.includes('/api/preventive-executions/') && url.endsWith('/submit')) {
        submittedUrls.push(url)
        return jsonResponse(buildExecutionDetails('execution-1', [
          {
            id: 'answer-1',
            itemKey: 'equipmentClean',
            value: 'true',
            comment: null,
          },
        ]))
      }

      return new Response(null, { status: 404 })
    })

    renderWithProviders(<PreventiveExecutionStartPage />, {
      route: '/app/preventive-executions/start',
    })

    await screen.findByRole('option', { name: 'UPS-01 - UPS' })
    await user.selectOptions(screen.getByLabelText('Inventory item'), 'inventory-1')
    await user.selectOptions(await screen.findByLabelText(/Equipment clean/i), 'true')
    await user.click(screen.getByRole('button', { name: /^submit$/i }))

    await waitFor(() => {
      expect(submittedUrls).toEqual([
        'http://localhost:5105/api/preventive-executions/execution-1/submit',
      ])
    })
  })
})

function buildExecutionDetails(id: string, answers: unknown[]) {
  return {
    id,
    inventoryItemId: 'inventory-1',
    inventoryItemDisplayName: 'UPS-01',
    preventiveTemplateId: 'template-1',
    preventiveTemplateName: 'Quarterly UPS Inspection',
    preventiveTemplateCode: 'quarterly-ups',
    entityTypeId: 'entity-type-1',
    entityTypeName: 'UPS',
    entityTypeCode: 'ups',
    regionId: 'region-1',
    regionName: 'North Region',
    siteId: 'site-1',
    siteName: 'North Hub',
    status: 'submitted',
    createdBy: 'admin-1',
    updatedBy: 'admin-1',
    submittedBy: 'admin-1',
    createdAtUtc: '2026-05-19T12:00:00Z',
    updatedAtUtc: '2026-05-19T12:00:00Z',
    submittedAtUtc: '2026-05-19T12:00:00Z',
    templateSections: [],
    answers,
    validationHistory: [],
  }
}

function jsonResponse(payload: unknown, status = 200) {
  return new Response(JSON.stringify(payload), {
    status,
    headers: {
      'Content-Type': 'application/json',
    },
  })
}
