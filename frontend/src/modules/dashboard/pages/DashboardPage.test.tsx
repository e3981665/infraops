import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import { DashboardPage } from '@/modules/dashboard/pages/DashboardPage'
import { routePaths } from '@/shared/routing/route-paths'

afterEach(() => {
  authTokenStorage.clear()
  vi.restoreAllMocks()
})

describe('DashboardPage', () => {
  it('should render dashboard summary cards and charts', async () => {
    arrangeAuthenticatedDashboardFetch({
      totalInventoryItems: 6,
      activeInventoryItems: 6,
      preventiveExecutionsThisMonth: 4,
      pendingValidationExecutions: 1,
      approvedPreventiveExecutions: 2,
      rejectedPreventiveExecutions: 1,
      reworkRequestedPreventiveExecutions: 1,
      activeEntityTypes: 3,
      activePreventiveTemplates: 3,
    }, {
      executionsByMonth: [{ label: '2026-05', value: 4 }],
      validationResultsByStatus: [{ label: 'Approved', value: 2 }],
      inventoryByEntityType: [{ label: 'UPS', value: 2 }],
      executionsByEntityType: [{ label: 'Generator', value: 2 }],
    })

    renderWithProviders(
      <Routes>
        <Route path={routePaths.app} element={<DashboardPage />} />
      </Routes>,
      { route: routePaths.app },
    )

    expect(await screen.findByText('Total inventory')).toBeInTheDocument()
    expect(screen.getAllByText('6').length).toBeGreaterThan(0)
    expect(screen.getByText('Preventive executions by month')).toBeInTheDocument()
    expect(screen.getByText('Validation results by status')).toBeInTheDocument()
    expect(screen.getByText('UPS')).toBeInTheDocument()
  })

  it('should render empty chart states', async () => {
    arrangeAuthenticatedDashboardFetch({
      totalInventoryItems: 0,
      activeInventoryItems: 0,
      preventiveExecutionsThisMonth: 0,
      pendingValidationExecutions: 0,
      approvedPreventiveExecutions: 0,
      rejectedPreventiveExecutions: 0,
      reworkRequestedPreventiveExecutions: 0,
      activeEntityTypes: 0,
      activePreventiveTemplates: 0,
    }, {
      executionsByMonth: [],
      validationResultsByStatus: [],
      inventoryByEntityType: [],
      executionsByEntityType: [],
    })

    renderWithProviders(
      <Routes>
        <Route path={routePaths.app} element={<DashboardPage />} />
      </Routes>,
      { route: routePaths.app },
    )

    expect(
      await screen.findByText('No execution activity exists for the current filters.'),
    ).toBeInTheDocument()
    expect(screen.getByText('No inventory exists for the current filters.')).toBeInTheDocument()
  })
})

function arrangeAuthenticatedDashboardFetch(summary: unknown, charts: unknown) {
  authTokenStorage.write({
    accessToken: 'access-token',
    accessTokenExpiresAtUtc: '2026-04-02T12:15:00Z',
    refreshToken: 'refresh-token',
    refreshTokenExpiresAtUtc: '2026-04-09T12:00:00Z',
  })

  vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = input.toString()

    if (url.includes('/api/auth/me')) {
      return jsonResponse({
        id: 'admin-1',
        fullName: 'InfraOps Administrator',
        email: 'admin@infraops.local',
        roles: ['Admin'],
        permissions: [permissionCodes.preventiveRead],
      })
    }

    if (url.includes('/api/dashboard/summary')) {
      return jsonResponse(summary)
    }

    return jsonResponse(charts)
  })
}

function jsonResponse(payload: unknown) {
  return new Response(JSON.stringify(payload), {
    status: 200,
    headers: {
      'Content-Type': 'application/json',
    },
  })
}
