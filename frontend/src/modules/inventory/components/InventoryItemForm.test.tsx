import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { InventoryItemForm } from '@/modules/inventory/components/InventoryItemForm'
import { createDefaultInventoryItemFormValues } from '@/modules/inventory/utils/inventory-form-utils'
import type {
  InventoryFormDefinition,
  InventoryFormMetadata,
} from '@/modules/inventory/types/inventory'

const metadata: InventoryFormMetadata = {
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
}

const formDefinition: InventoryFormDefinition = {
  entityTypeId: '26043c08-0880-46d9-b7dc-5778d07d64a9',
  entityTypeName: 'UPS',
  entityTypeCode: 'ups',
  fieldDefinitions: [
    {
      id: 'ebdb3986-f44a-4790-8d84-59710995fc64',
      fieldKey: 'serialNumber',
      displayLabel: 'Serial Number',
      fieldType: 'text',
      displayOrder: 1,
      isRequired: true,
      placeholder: 'Printed on the nameplate',
      helpText: 'Used across inventory and maintenance flows.',
      options: [],
    },
    {
      id: 'ed5ea97d-5364-4946-8f2b-33427f543197',
      fieldKey: 'phaseType',
      displayLabel: 'Phase Type',
      fieldType: 'select',
      displayOrder: 2,
      isRequired: false,
      placeholder: null,
      helpText: null,
      options: [
        {
          id: 'b0976553-5c6e-4279-9a3c-420980f68456',
          value: 'single-phase',
          label: 'Single-phase',
          displayOrder: 1,
        },
        {
          id: '073f94ae-c952-4824-8a22-c8ba8d0e411d',
          value: 'three-phase',
          label: 'Three-phase',
          displayOrder: 2,
        },
      ],
    },
  ],
}

describe('InventoryItemForm', () => {
  it('should render dynamic fields from the selected entity type definition', () => {
    render(
      <InventoryItemForm
        eyebrow="Inventory management"
        title="Register inventory item"
        description="Configure the infrastructure item."
        submitLabel="Save inventory item"
        initialValues={{
          ...createDefaultInventoryItemFormValues(metadata),
          entityTypeId: metadata.entityTypes[0]!.id,
          regionId: metadata.regions[0]!.id,
          siteId: metadata.sites[0]!.id,
        }}
        metadata={metadata}
        formDefinition={formDefinition}
        isFormDefinitionLoading={false}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByLabelText(/serial number/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/phase type/i)).toBeInTheDocument()
    expect(screen.getByText('Used across inventory and maintenance flows.')).toBeInTheDocument()
  })

  it('should render select field options correctly', () => {
    render(
      <InventoryItemForm
        eyebrow="Inventory management"
        title="Register inventory item"
        description="Configure the infrastructure item."
        submitLabel="Save inventory item"
        initialValues={{
          ...createDefaultInventoryItemFormValues(metadata),
          entityTypeId: metadata.entityTypes[0]!.id,
          regionId: metadata.regions[0]!.id,
          siteId: metadata.sites[0]!.id,
        }}
        metadata={metadata}
        formDefinition={formDefinition}
        isFormDefinitionLoading={false}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByRole('option', { name: 'Single-phase' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Three-phase' })).toBeInTheDocument()
  })

  it('should validate required fields before submission', async () => {
    const user = userEvent.setup()

    render(
      <InventoryItemForm
        eyebrow="Inventory management"
        title="Register inventory item"
        description="Configure the infrastructure item."
        submitLabel="Save inventory item"
        initialValues={{
          ...createDefaultInventoryItemFormValues(metadata),
          entityTypeId: metadata.entityTypes[0]!.id,
          regionId: metadata.regions[0]!.id,
          siteId: metadata.sites[0]!.id,
        }}
        metadata={metadata}
        formDefinition={formDefinition}
        isFormDefinitionLoading={false}
        onSubmit={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.click(screen.getByRole('button', { name: /save inventory item/i }))

    await waitFor(() => {
      expect(screen.getByText('Display name is required.')).toBeInTheDocument()
    })

    expect(screen.getByText('Serial Number is required.')).toBeInTheDocument()
  })
})
