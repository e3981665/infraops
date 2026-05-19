import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { preventiveTemplateQueryKeys } from '@/modules/preventive-templates/api/preventive-template-query-keys'
import { preventiveTemplatesClient } from '@/modules/preventive-templates/api/preventive-templates-client'
import { PreventiveTemplateForm } from '@/modules/preventive-templates/components/PreventiveTemplateForm'
import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import { buildPreventiveTemplateDetailPath } from '@/shared/routing/route-paths'
import {
  ensureTemplateEntityTypeOption,
  mapPreventiveTemplateToFormValues,
} from '@/modules/preventive-templates/utils/preventive-template-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function PreventiveTemplateEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { preventiveTemplateId } = useParams()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const templateQuery = useQuery({
    queryKey: preventiveTemplateId
      ? preventiveTemplateQueryKeys.detail(preventiveTemplateId)
      : preventiveTemplateQueryKeys.all,
    queryFn: () => preventiveTemplatesClient.getById(preventiveTemplateId!, accessToken!),
    enabled: Boolean(preventiveTemplateId && accessToken),
  })

  const metadataQuery = useQuery({
    queryKey: preventiveTemplateQueryKeys.formMetadata,
    queryFn: () => preventiveTemplatesClient.getFormMetadata(accessToken!),
    enabled: Boolean(accessToken),
  })

  const updateMutation = useMutation({
    mutationFn: (values: PreventiveTemplateFormValues) =>
      preventiveTemplatesClient.update(preventiveTemplateId!, values, accessToken!),
    onSuccess: async (preventiveTemplate) => {
      await queryClient.invalidateQueries({ queryKey: preventiveTemplateQueryKeys.lists() })
      queryClient.setQueryData(
        preventiveTemplateQueryKeys.detail(preventiveTemplate.id),
        preventiveTemplate,
      )
      navigate(buildPreventiveTemplateDetailPath(preventiveTemplate.id), { replace: true })
    },
  })

  if (!preventiveTemplateId || !accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.detail.unresolvedTitle')}</h1>
        <p>{t('templates.edit.unresolvedHelp')}</p>
      </section>
    )
  }

  if (templateQuery.isLoading || metadataQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.detail.loadingTitle')}</h1>
        <p>{t('templates.edit.loadingHelp')}</p>
      </section>
    )
  }

  if (templateQuery.isError || !templateQuery.data || metadataQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.detail.loadFailed')}</h1>
        <p>{templateQuery.error?.message ?? metadataQuery.error?.message}</p>
      </section>
    )
  }

  const template = templateQuery.data
  const entityTypeOptions = ensureTemplateEntityTypeOption(metadataQuery.data.entityTypes, {
    id: template.entityTypeId,
    code: template.entityTypeCode,
    name: template.entityTypeName,
  })

  return (
    <PreventiveTemplateForm
      eyebrow={t('templates.detail.administration')}
      title={t('templates.edit.title', { name: template.name })}
      description={t('templates.edit.description')}
      submitLabel={t('common.save')}
      initialValues={mapPreventiveTemplateToFormValues(template)}
      entityTypeOptions={entityTypeOptions}
      isEntityTypeLocked
      onSubmit={async (values) => {
        await updateMutation.mutateAsync(values)
      }}
    />
  )
}
