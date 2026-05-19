import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import { InventoryItemForm } from '@/modules/inventory/components/InventoryItemForm'
import type { InventoryItemFormValues } from '@/modules/inventory/schemas/inventory-form-schema'
import { buildInventoryDetailPath } from '@/shared/routing/route-paths'
import { createDefaultInventoryItemFormValues } from '@/modules/inventory/utils/inventory-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function InventoryCreatePage() {
  const { t } = useTranslation()
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

  const initialValues = useMemo(
    () =>
      metadataQuery.data
        ? createDefaultInventoryItemFormValues(metadataQuery.data)
        : null,
    [metadataQuery.data],
  )

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
        <p>{t('inventory.authMessage')}</p>
      </section>
    )
  }

  if (metadataQuery.isLoading || !initialValues) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.loadingMetadata')}</h1>
        <p>{t('inventory.loadingMetadataHelp')}</p>
      </section>
    )
  }

  if (metadataQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.metadataLoadFailed')}</h1>
        <p>{metadataQuery.error?.message ?? t('inventory.lookupDataMissing')}</p>
      </section>
    )
  }

  return (
    <InventoryItemForm
      eyebrow={t('inventory.management')}
      title={t('inventory.createTitle')}
      description={t('inventory.createDescription')}
      submitLabel={t('inventory.saveItem')}
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
