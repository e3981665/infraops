import { screen, waitFor } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { PreventiveValidationDetailPage } from '@/modules/preventive-validations/pages/PreventiveValidationDetailPage'
import { PreventiveValidationQueuePage } from '@/modules/preventive-validations/pages/PreventiveValidationQueuePage'
import { routePaths } from '@/shared/routing/route-paths'

const executionSummary = {
  id: 'execution-1',
  inventoryItemId: 'inventory-1',
  inventoryItemDisplayName: 'UPS-01',
  preventiveTemplateId: 'template-1',
  preventiveTemplateName: 'Quarterly UPS Inspection',
  entityTypeId: 'entity-type-1',
  entityTypeName: 'UPS',
  regionId: 'region-1',
  regionName: 'North Region',
  siteId: 'site-1',
  siteName: 'North Hub',
  status: 'submitted',
  createdBy: 'user-1',
  updatedBy: 'user-1',
  submittedBy: 'user-1',
  createdAtUtc: '2026-04-02T12:00:00Z',
  updatedAtUtc: '2026-04-02T12:10:00Z',
  submittedAtUtc: '2026-04-02T12:10:00Z',
}

const executionDetail = {
  ...executionSummary,
  preventiveTemplateCode: 'quarterly-ups-inspection',
  entityTypeCode: 'ups',
  templateSections: [
    {
      id: 'section-1',
      sourceTemplateSectionId: 'source-section-1',
      title: 'Visual Inspection',
      displayOrder: 1,
      checklistItems: [
        {
          id: 'item-1',
          sourceChecklistItemId: 'source-item-1',
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
  answers: [
    {
      id: 'answer-1',
      itemKey: 'equipmentClean',
      value: 'true',
      comment: null,
    },
  ],
  validationHistory: [],
}

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('PreventiveValidationPages', () => {
  it('should render submitted executions in the validation queue', async () => {
    arrangeAuthenticatedFetch([executionSummary])

    renderWithProviders(
      <Routes>
        <Route path={routePaths.preventiveValidations} element={<PreventiveValidationQueuePage />} />
      </Routes>,
      { route: routePaths.preventiveValidations },
    )

    expect(await screen.findByText('UPS-01')).toBeInTheDocument()
    expect(screen.getByText('Quarterly UPS Inspection')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /review/i })).toBeInTheDocument()
  })

  it('should render execution snapshot and answers on validation detail', async () => {
    arrangeAuthenticatedFetch(executionDetail)

    renderWithProviders(
      <Routes>
        <Route
          path={routePaths.preventiveValidationDetail}
          element={<PreventiveValidationDetailPage />}
        />
      </Routes>,
      { route: '/app/preventive-validations/execution-1' },
    )

    expect(await screen.findByText('UPS-01')).toBeInTheDocument()
    expect(screen.getByText('Visual Inspection')).toBeInTheDocument()
    expect(screen.getByLabelText(/Equipment clean/i)).toBeInTheDocument()
    expect(screen.getByText('No validation actions have been recorded.')).toBeInTheDocument()
  })

  it('should block users who do not have validation permission', async () => {
    authTokenStorage.write(createTokens())

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      jsonResponse({
        id: 'viewer-1',
        fullName: 'InfraOps Viewer',
        email: 'viewer@infraops.local',
        roles: ['Viewer'],
        permissions: [],
      }),
    )

    renderWithProviders(
      <Routes>
        <Route
          path={routePaths.preventiveValidations}
          element={
            <ProtectedRoute requiredPermission={permissionCodes.preventiveValidate}>
              <div>Preventive validation area</div>
            </ProtectedRoute>
          }
        />
      </Routes>,
      { route: routePaths.preventiveValidations },
    )

    await waitFor(() => {
      expect(screen.getByText('Permission required.')).toBeInTheDocument()
    })

    expect(screen.queryByText('Preventive validation area')).not.toBeInTheDocument()
  })
})

function arrangeAuthenticatedFetch(apiPayload: unknown) {
  authTokenStorage.write(createTokens())

  vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = input.toString()

    if (url.includes('/api/auth/me')) {
      return jsonResponse({
        id: 'validator-1',
        fullName: 'InfraOps Validator',
        email: 'validator@infraops.local',
        roles: ['Validator'],
        permissions: [permissionCodes.preventiveValidate],
      })
    }

    return jsonResponse(apiPayload)
  })
}

function createTokens() {
  return {
    accessToken: 'access-token',
    accessTokenExpiresAtUtc: '2026-04-02T12:15:00Z',
    refreshToken: 'refresh-token',
    refreshTokenExpiresAtUtc: '2026-04-09T12:00:00Z',
  }
}

function jsonResponse(payload: unknown) {
  return new Response(JSON.stringify(payload), {
    status: 200,
    headers: {
      'Content-Type': 'application/json',
    },
  })
}
