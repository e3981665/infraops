import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { EntityTypeForm } from '@/modules/entity-types/components/EntityTypeForm'
import { entityTypeQueryKeys } from '@/modules/entity-types/api/entity-type-query-keys'
import { entityTypesClient } from '@/modules/entity-types/api/entity-types-client'
import type { EntityTypeFormValues } from '@/modules/entity-types/schemas/entity-type-form-schema'
import { buildEntityTypeDetailPath } from '@/shared/routing/route-paths'
import { mapEntityTypeToFormValues } from '@/modules/entity-types/utils/entity-type-form-utils'

export function EntityTypeEditPage() {
  const navigate = useNavigate()
  const { entityTypeId } = useParams()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const entityTypeQuery = useQuery({
    queryKey: entityTypeId ? entityTypeQueryKeys.detail(entityTypeId) : entityTypeQueryKeys.all,
    queryFn: () => entityTypesClient.getById(entityTypeId!, accessToken!),
    enabled: Boolean(entityTypeId && accessToken),
  })

  const updateEntityTypeMutation = useMutation({
    mutationFn: (values: EntityTypeFormValues) =>
      entityTypesClient.update(entityTypeId!, values, accessToken!),
    onSuccess: async (entityType) => {
      await queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.all })
      queryClient.setQueryData(entityTypeQueryKeys.detail(entityType.id), entityType)
      navigate(buildEntityTypeDetailPath(entityType.id), { replace: true })
    },
  })

  if (!entityTypeId || !accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Entity type could not be resolved.</h1>
        <p>The edit route requires a valid entity type id and an authenticated session.</p>
      </section>
    )
  }

  if (entityTypeQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Loading entity definition.</h1>
        <p>InfraOps is fetching the current configuration before editing.</p>
      </section>
    )
  }

  if (entityTypeQuery.isError || !entityTypeQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Entity definition could not be loaded.</h1>
        <p>{entityTypeQuery.error?.message ?? 'The entity type was not found.'}</p>
      </section>
    )
  }

  return (
    <EntityTypeForm
      eyebrow="Entity type administration"
      title={`Edit ${entityTypeQuery.data.name}`}
      description="Update metadata, field configuration, and activation flags through one aggregate edit flow."
      submitLabel="Save changes"
      initialValues={mapEntityTypeToFormValues(entityTypeQuery.data)}
      onSubmit={async (values) => {
        await updateEntityTypeMutation.mutateAsync(values)
      }}
    />
  )
}
