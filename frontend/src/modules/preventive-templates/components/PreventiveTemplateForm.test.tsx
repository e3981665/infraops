import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { PreventiveTemplateForm } from '@/modules/preventive-templates/components/PreventiveTemplateForm'
import {
  createDefaultPreventiveTemplateFormValues,
  mapPreventiveTemplateToFormValues,
} from '@/modules/preventive-templates/utils/preventive-template-form-utils'
import { renderWithProviders } from '@/app/test/render-with-providers'

const entityTypeOptions = [
  {
    id: '26043c08-0880-46d9-b7dc-5778d07d64a9',
    code: 'ups',
    name: 'UPS',
  },
]

describe('PreventiveTemplateForm', () => {
  it('should validate required fields before submission', async () => {
    const user = userEvent.setup()

    renderWithProviders(
      <PreventiveTemplateForm
        eyebrow="Preventive template administration"
        title="Create preventive template"
        description="Configure a preventive template."
        submitLabel="Save preventive template"
        initialValues={{
          ...createDefaultPreventiveTemplateFormValues({ entityTypes: entityTypeOptions }),
          entityTypeId: '',
        }}
        entityTypeOptions={entityTypeOptions}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.click(screen.getByRole('button', { name: /save preventive template/i }))

    await waitFor(() => {
      expect(screen.getByText('Entity type is required.')).toBeInTheDocument()
    })

    expect(screen.getByText('Template name is required.')).toBeInTheDocument()
    expect(screen.getByText('Template code is required.')).toBeInTheDocument()
    expect(screen.getByText('Section title is required.')).toBeInTheDocument()
    expect(screen.getByText('Checklist item key is required.')).toBeInTheDocument()
  })

  it('should show the options editor when the item type is select', async () => {
    const user = userEvent.setup()

    renderWithProviders(
      <PreventiveTemplateForm
        eyebrow="Preventive template administration"
        title="Create preventive template"
        description="Configure a preventive template."
        submitLabel="Save preventive template"
        initialValues={createDefaultPreventiveTemplateFormValues({ entityTypes: entityTypeOptions })}
        entityTypeOptions={entityTypeOptions}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.selectOptions(screen.getByLabelText('Item type'), 'select')

    expect(screen.getByText('Select options')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add option/i })).toBeInTheDocument()
  })

  it('should show numeric bounds when the item type is numeric', async () => {
    const user = userEvent.setup()

    renderWithProviders(
      <PreventiveTemplateForm
        eyebrow="Preventive template administration"
        title="Create preventive template"
        description="Configure a preventive template."
        submitLabel="Save preventive template"
        initialValues={createDefaultPreventiveTemplateFormValues({ entityTypes: entityTypeOptions })}
        entityTypeOptions={entityTypeOptions}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.selectOptions(screen.getByLabelText('Item type'), 'numeric')

    expect(screen.getByLabelText('Minimum value')).toBeInTheDocument()
    expect(screen.getByLabelText('Maximum value')).toBeInTheDocument()
  })

  it('should hide the options editor when the item type is changed away from select', async () => {
    const user = userEvent.setup()

    renderWithProviders(
      <PreventiveTemplateForm
        eyebrow="Preventive template administration"
        title="Edit preventive template"
        description="Configure a preventive template."
        submitLabel="Save changes"
        initialValues={mapPreventiveTemplateToFormValues({
          id: '6202f4f0-e5e1-4bf0-b871-fc44e0ef8dd3',
          entityTypeId: entityTypeOptions[0]!.id,
          entityTypeName: 'UPS',
          entityTypeCode: 'ups',
          name: 'UPS Inspection',
          code: 'ups-inspection',
          description: 'Quarterly preventive checklist.',
          isActive: true,
          sections: [
            {
              id: 'd1ef23e8-2f8a-4a9d-88aa-53dd208310b1',
              title: 'Measurements',
              displayOrder: 1,
              isActive: true,
              checklistItems: [
                {
                  id: 'bc259799-c11b-4eed-902c-f885b4bbda1b',
                  itemKey: 'batteryCondition',
                  label: 'Battery condition',
                  itemType: 'select',
                  displayOrder: 1,
                  isRequired: true,
                  isActive: true,
                  helpText: null,
                  isCritical: false,
                  requiresCommentOnFailure: true,
                  requiresPhotoOnFailure: false,
                  minimumValue: null,
                  maximumValue: null,
                  options: [
                    {
                      id: '98ec1caa-814f-4930-8d77-6737f50d0f53',
                      value: 'good',
                      label: 'Good',
                      displayOrder: 1,
                    },
                  ],
                },
              ],
            },
          ],
        })}
        entityTypeOptions={entityTypeOptions}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Select options')).toBeInTheDocument()

    await user.selectOptions(screen.getByLabelText('Item type'), 'yesNo')

    await waitFor(() => {
      expect(screen.queryByText('Select options')).not.toBeInTheDocument()
    })
  })
})
