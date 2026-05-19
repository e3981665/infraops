import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { EntityTypeForm } from '@/modules/entity-types/components/EntityTypeForm'
import { entityTypeQueryKeys } from '@/modules/entity-types/api/entity-type-query-keys'
import { entityTypesClient } from '@/modules/entity-types/api/entity-types-client'
import type { EntityTypeFormValues } from '@/modules/entity-types/schemas/entity-type-form-schema'
import { buildEntityTypeDetailPath } from '@/shared/routing/route-paths'
import { mapEntityTypeToFormValues } from '@/modules/entity-types/utils/entity-type-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function EntityTypeEditPage() {
  const { t } = useTranslation()
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
        <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
        <h1>{t('entity.detail.unresolvedTitle')}</h1>
        <p>{t('entity.edit.unresolvedHelp')}</p>
      </section>
    )
  }

  if (entityTypeQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
        <h1>{t('entity.detail.loadingTitle')}</h1>
        <p>{t('entity.edit.loadingHelp')}</p>
      </section>
    )
  }

  if (entityTypeQuery.isError || !entityTypeQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
        <h1>{t('entity.detail.loadFailed')}</h1>
        <p>{entityTypeQuery.error?.message ?? t('entity.detail.notFound')}</p>
      </section>
    )
  }

  return (
    <EntityTypeForm
      eyebrow={t('entity.detail.administration')}
      title={t('entity.edit.title', { name: entityTypeQuery.data.name })}
      description={t('entity.edit.description')}
      submitLabel={t('common.save')}
      initialValues={mapEntityTypeToFormValues(entityTypeQuery.data)}
      onSubmit={async (values) => {
        await updateEntityTypeMutation.mutateAsync(values)
      }}
    />
  )
}
