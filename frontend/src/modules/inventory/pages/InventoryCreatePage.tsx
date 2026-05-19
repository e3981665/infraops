import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import { InventoryItemForm } from '@/modules/inventory/components/InventoryItemForm'
import type { InventoryItemFormValues } from '@/modules/inventory/schemas/inventory-form-schema'
import { buildInventoryDetailPath } from '@/shared/routing/route-paths'
import { createDefaultInventoryItemFormValues } from '@/modules/inventory/utils/inventory-form-utils'

export function InventoryCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken
  const [selectedEntityTypeId, setSelectedEntityTypeId] = useState('')

  const metadataQuery = useQuery({
    queryKey: inventoryQueryKeys.formMetadata,
    queryFn: () => inventoryClient.getFormMetadata(accessToken!),
    enabled: Boolean(accessToken),
  })

  const formDefinitionQuery = useQuery({
    queryKey: selectedEntityTypeId
      ? inventoryQueryKeys.formDefinition(selectedEntityTypeId)
      : inventoryQueryKeys.all,
    queryFn: () => inventoryClient.getFormDefinition(selectedEntityTypeId, accessToken!),
    enabled: Boolean(accessToken && selectedEntityTypeId),
  })

  const createInventoryItemMutation = useMutation({
    mutationFn: (values: InventoryItemFormValues) =>
      inventoryClient.create(values, formDefinitionQuery.data!, accessToken!),
    onSuccess: async (inventoryItem) => {
      await queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.lists() })
      queryClient.setQueryData(inventoryQueryKeys.detail(inventoryItem.id), inventoryItem)
      navigate(buildInventoryDetailPath(inventoryItem.id), { replace: true })
    },
  })

  const initialValues = metadataQuery.data
    ? createDefaultInventoryItemFormValues(metadataQuery.data)
    : null

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Authenticated access is required.</h1>
        <p>The inventory workspace is waiting for a valid session.</p>
      </section>
    )
  }

  if (metadataQuery.isLoading || !initialValues) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Loading inventory metadata.</h1>
        <p>InfraOps is preparing the dynamic registration form.</p>
      </section>
    )
  }

  if (metadataQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Inventory metadata could not be loaded.</h1>
        <p>{metadataQuery.error?.message ?? 'The required lookup data was not found.'}</p>
      </section>
    )
  }

  return (
    <InventoryItemForm
      eyebrow="Inventory management"
      title="Register a new inventory item"
      description="Choose an entity type, load its dynamic fields, and store the infrastructure item through one consistent aggregate flow."
      submitLabel="Save inventory item"
      initialValues={initialValues}
      metadata={metadataQuery.data}
      formDefinition={formDefinitionQuery.data ?? null}
      isFormDefinitionLoading={formDefinitionQuery.isLoading}
      onEntityTypeChange={(entityTypeId) => {
        setSelectedEntityTypeId(entityTypeId)
      }}
      onSubmit={async (values) => {
        await createInventoryItemMutation.mutateAsync(values)
      }}
    />
  )
}
