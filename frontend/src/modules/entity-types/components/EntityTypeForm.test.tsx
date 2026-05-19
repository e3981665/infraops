import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { EntityTypeForm } from '@/modules/entity-types/components/EntityTypeForm'
import {
  createDefaultEntityTypeFormValues,
  mapEntityTypeToFormValues,
} from '@/modules/entity-types/utils/entity-type-form-utils'

afterEach(() => {
  vi.restoreAllMocks()
})

describe('EntityTypeForm', () => {
  it('should validate required fields before submission', async () => {
    const user = userEvent.setup()

    render(
      <EntityTypeForm
        eyebrow="Entity type administration"
        title="Create entity type"
        description="Configure a dynamic entity definition."
        submitLabel="Save entity type"
        initialValues={createDefaultEntityTypeFormValues()}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.click(screen.getByRole('button', { name: /save entity type/i }))

    expect(screen.getByText('Entity type name is required.')).toBeInTheDocument()
    expect(screen.getByText('Entity type code is required.')).toBeInTheDocument()
    expect(screen.getByText('Field key is required.')).toBeInTheDocument()
  })

  it('should show the options editor when the field type is select', async () => {
    const user = userEvent.setup()

    render(
      <EntityTypeForm
        eyebrow="Entity type administration"
        title="Create entity type"
        description="Configure a dynamic entity definition."
        submitLabel="Save entity type"
        initialValues={createDefaultEntityTypeFormValues()}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.selectOptions(screen.getByLabelText('Field type'), 'select')

    expect(screen.getByText('Select options')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add option/i })).toBeInTheDocument()
  })

  it('should hide the options editor when the field type is changed away from select', async () => {
    const user = userEvent.setup()

    render(
      <EntityTypeForm
        eyebrow="Entity type administration"
        title="Edit entity type"
        description="Configure a dynamic entity definition."
        submitLabel="Save changes"
        initialValues={mapEntityTypeToFormValues({
          id: '6202f4f0-e5e1-4bf0-b871-fc44e0ef8dd3',
          name: 'UPS',
          code: 'ups',
          description: 'Critical power backup assets.',
          isActive: true,
          fieldDefinitions: [
            {
              id: '0fa783fd-55b2-4510-9b6b-b876c86f0d4f',
              fieldKey: 'phaseType',
              displayLabel: 'Phase Type',
              fieldType: 'select',
              displayOrder: 1,
              isRequired: true,
              isActive: true,
              placeholder: null,
              helpText: null,
              options: [
                {
                  id: '98ec1caa-814f-4930-8d77-6737f50d0f53',
                  value: 'single-phase',
                  label: 'Single-phase',
                  displayOrder: 1,
                },
              ],
            },
          ],
        })}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Select options')).toBeInTheDocument()

    await user.selectOptions(screen.getByLabelText('Field type'), 'text')

    await waitFor(() => {
      expect(screen.queryByText('Select options')).not.toBeInTheDocument()
    })
  })
})
