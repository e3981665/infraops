import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import { InventoryItemForm } from '@/modules/inventory/components/InventoryItemForm'
import type { InventoryItemFormValues } from '@/modules/inventory/schemas/inventory-form-schema'
import { buildInventoryDetailPath } from '@/shared/routing/route-paths'
import { mapInventoryItemToFormValues } from '@/modules/inventory/utils/inventory-form-utils'

export function InventoryEditPage() {
  const navigate = useNavigate()
  const { inventoryItemId } = useParams()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const inventoryItemQuery = useQuery({
    queryKey: inventoryItemId ? inventoryQueryKeys.detail(inventoryItemId) : inventoryQueryKeys.all,
    queryFn: () => inventoryClient.getById(inventoryItemId!, accessToken!),
    enabled: Boolean(inventoryItemId && accessToken),
  })

  const metadataQuery = useQuery({
    queryKey: inventoryQueryKeys.formMetadata,
    queryFn: () => inventoryClient.getFormMetadata(accessToken!),
    enabled: Boolean(accessToken),
  })

  const formDefinitionQuery = useQuery({
    queryKey:
      inventoryItemQuery.data?.entityTypeId
        ? inventoryQueryKeys.formDefinition(inventoryItemQuery.data.entityTypeId)
        : inventoryQueryKeys.all,
    queryFn: () =>
      inventoryClient.getFormDefinition(inventoryItemQuery.data!.entityTypeId, accessToken!),
    enabled: Boolean(accessToken && inventoryItemQuery.data?.entityTypeId),
  })

  const updateInventoryItemMutation = useMutation({
    mutationFn: (values: InventoryItemFormValues) =>
      inventoryClient.update(inventoryItemId!, values, formDefinitionQuery.data!, accessToken!),
    onSuccess: async (inventoryItem) => {
      await queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.lists() })
      queryClient.setQueryData(inventoryQueryKeys.detail(inventoryItem.id), inventoryItem)
      navigate(buildInventoryDetailPath(inventoryItem.id), { replace: true })
    },
  })

  const initialValues = inventoryItemQuery.data
    ? mapInventoryItemToFormValues(inventoryItemQuery.data)
    : null

  if (!inventoryItemId || !accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Inventory item could not be resolved.</h1>
        <p>The edit route requires a valid item id and an authenticated session.</p>
      </section>
    )
  }

  if (inventoryItemQuery.isLoading || metadataQuery.isLoading || !initialValues) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Loading inventory item.</h1>
        <p>InfraOps is fetching the current item and dynamic definition before editing.</p>
      </section>
    )
  }

  if (inventoryItemQuery.isError || metadataQuery.isError || !inventoryItemQuery.data || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Inventory item could not be loaded.</h1>
        <p>{inventoryItemQuery.error?.message ?? metadataQuery.error?.message}</p>
      </section>
    )
  }

  return (
    <InventoryItemForm
      eyebrow="Inventory management"
      title={`Edit ${inventoryItemQuery.data.displayName}`}
      description="Update operational metadata and dynamic attributes while preserving the original entity type association."
      submitLabel="Save changes"
      initialValues={initialValues}
      metadata={metadataQuery.data}
      formDefinition={formDefinitionQuery.data ?? null}
      isFormDefinitionLoading={formDefinitionQuery.isLoading}
      isEntityTypeLocked
      onSubmit={async (values) => {
        await updateInventoryItemMutation.mutateAsync(values)
      }}
    />
  )
}
