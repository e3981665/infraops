import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { EntityTypeForm } from '@/modules/entity-types/components/EntityTypeForm'
import { entityTypeQueryKeys } from '@/modules/entity-types/api/entity-type-query-keys'
import { entityTypesClient } from '@/modules/entity-types/api/entity-types-client'
import type { EntityTypeFormValues } from '@/modules/entity-types/schemas/entity-type-form-schema'
import { buildEntityTypeDetailPath } from '@/shared/routing/route-paths'
import { createDefaultEntityTypeFormValues } from '@/modules/entity-types/utils/entity-type-form-utils'

export function EntityTypeCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const createEntityTypeMutation = useMutation({
    mutationFn: (values: EntityTypeFormValues) => entityTypesClient.create(values, accessToken!),
    onSuccess: async (entityType) => {
      await queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.all })
      queryClient.setQueryData(entityTypeQueryKeys.detail(entityType.id), entityType)
      navigate(buildEntityTypeDetailPath(entityType.id), { replace: true })
    },
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Authenticated access is required.</h1>
        <p>The admin workspace is waiting for a valid session.</p>
      </section>
    )
  }

  return (
    <EntityTypeForm
      eyebrow="Entity type administration"
      title="Create a new entity type"
      description="Define the metadata and dynamic fields that later inventory items will consume."
      submitLabel="Save entity type"
      initialValues={createDefaultEntityTypeFormValues()}
      onSubmit={async (values) => {
        await createEntityTypeMutation.mutateAsync(values)
      }}
    />
  )
}
