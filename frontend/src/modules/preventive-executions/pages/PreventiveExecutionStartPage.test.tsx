import type { PropsWithChildren, ReactElement } from 'react'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { AuthSessionContext } from '@/modules/auth/context/auth-session-context'
import { PreventiveExecutionStartPage } from '@/modules/preventive-executions/pages/PreventiveExecutionStartPage'
import type {
  PreventiveExecutionAnswer,
  PreventiveExecutionDetails,
} from '@/modules/preventive-executions/types/preventive-execution'
import { I18nProvider } from '@/shared/i18n/I18nProvider'
import { ThemeProvider } from '@/shared/theme/ThemeProvider'

type ClientHandler = (...args: unknown[]) => Promise<unknown>

const clientHandlers = vi.hoisted<Record<string, ClientHandler>>(() => ({
  inventoryList: async () => [],
  getFormDefinition: async () => {
    throw new Error('getFormDefinition handler was not configured.')
  },
  saveDraft: async () => {
    throw new Error('saveDraft handler was not configured.')
  },
  start: async () => {
    throw new Error('start handler was not configured.')
  },
  submit: async () => {
    throw new Error('submit handler was not configured.')
  },
}))

vi.mock('@/modules/inventory/api/inventory-client', () => ({
  inventoryClient: {
    list: (...args: unknown[]) => clientHandlers.inventoryList(...args),
  },
}))

vi.mock('@/modules/preventive-executions/api/preventive-executions-client', () => ({
  preventiveExecutionsClient: {
    getFormDefinition: (...args: unknown[]) => clientHandlers.getFormDefinition(...args),
    saveDraft: (...args: unknown[]) => clientHandlers.saveDraft(...args),
    start: (...args: unknown[]) => clientHandlers.start(...args),
    submit: (...args: unknown[]) => clientHandlers.submit(...args),
  },
}))

afterEach(() => {
  clientHandlers.inventoryList = async () => []
  clientHandlers.getFormDefinition = async () => {
    throw new Error('getFormDefinition handler was not configured.')
  }
  clientHandlers.saveDraft = async () => {
    throw new Error('saveDraft handler was not configured.')
  }
  clientHandlers.start = async () => {
    throw new Error('start handler was not configured.')
  }
  clientHandlers.submit = async () => {
    throw new Error('submit handler was not configured.')
  }
  vi.restoreAllMocks()
})

describe('PreventiveExecutionStartPage', () => {
  it('should submit a newly started execution with the returned execution id', async () => {
    const user = userEvent.setup()
    const submittedUrls: string[] = []

    clientHandlers.inventoryList = async () => [
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
    ]

    clientHandlers.getFormDefinition = async () => ({
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

    clientHandlers.start = async () => buildExecutionDetails('execution-1', [])

    clientHandlers.submit = async (executionId: unknown) => {
      submittedUrls.push(`http://localhost:5105/api/preventive-executions/${executionId}/submit`)

      return buildExecutionDetails('execution-1', [
          {
            id: 'answer-1',
            itemKey: 'equipmentClean',
            value: 'true',
            comment: null,
          },
        ])
    }

    renderWithAuthenticatedProviders(<PreventiveExecutionStartPage />, {
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

function renderWithAuthenticatedProviders(
  ui: ReactElement,
  options: { route: string },
) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  function Providers({ children }: PropsWithChildren) {
    return (
      <QueryClientProvider client={queryClient}>
        <ThemeProvider>
          <I18nProvider>
            <AuthSessionContext.Provider
              value={{
                status: 'authenticated',
                isAuthenticated: true,
                session: {
                  currentUser: {
                    id: 'admin-1',
                    fullName: 'InfraOps Administrator',
                    email: 'admin@infraops.local',
                    roles: ['Admin'],
                    permissions: ['preventive.execute', 'preventive.read'],
                  },
                  tokens: {
                    accessToken: 'access-token',
                    accessTokenExpiresAtUtc: '2026-05-19T12:15:00Z',
                    refreshToken: 'refresh-token',
                    refreshTokenExpiresAtUtc: '2026-05-26T12:00:00Z',
                  },
                },
                signIn: vi.fn(),
                signOut: vi.fn(),
                hasPermission: (permissionCode) =>
                  ['preventive.execute', 'preventive.read'].includes(permissionCode),
              }}
            >
              <MemoryRouter initialEntries={[options.route]}>{children}</MemoryRouter>
            </AuthSessionContext.Provider>
          </I18nProvider>
        </ThemeProvider>
      </QueryClientProvider>
    )
  }

  return render(ui, { wrapper: Providers })
}

function buildExecutionDetails(
  id: string,
  answers: PreventiveExecutionAnswer[],
): PreventiveExecutionDetails {
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
