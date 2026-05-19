import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { preventiveTemplateQueryKeys } from '@/modules/preventive-templates/api/preventive-template-query-keys'
import { preventiveTemplatesClient } from '@/modules/preventive-templates/api/preventive-templates-client'
import { PreventiveTemplateForm } from '@/modules/preventive-templates/components/PreventiveTemplateForm'
import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import {
  buildPreventiveTemplateDetailPath,
} from '@/shared/routing/route-paths'
import { createDefaultPreventiveTemplateFormValues } from '@/modules/preventive-templates/utils/preventive-template-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function PreventiveTemplateCreatePage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const metadataQuery = useQuery({
    queryKey: preventiveTemplateQueryKeys.formMetadata,
    queryFn: () => preventiveTemplatesClient.getFormMetadata(accessToken!),
    enabled: Boolean(accessToken),
  })

  const createMutation = useMutation({
    mutationFn: (values: PreventiveTemplateFormValues) =>
      preventiveTemplatesClient.create(values, accessToken!),
    onSuccess: async (preventiveTemplate) => {
      await queryClient.invalidateQueries({ queryKey: preventiveTemplateQueryKeys.lists() })
      queryClient.setQueryData(
        preventiveTemplateQueryKeys.detail(preventiveTemplate.id),
        preventiveTemplate,
      )
      navigate(buildPreventiveTemplateDetailPath(preventiveTemplate.id), { replace: true })
    },
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('common.authRequired')}</h1>
        <p>{t('templates.authMessage')}</p>
      </section>
    )
  }

  if (metadataQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.metadataLoading')}</h1>
        <p>{t('templates.metadataLoadingHelp')}</p>
      </section>
    )
  }

  if (metadataQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.metadataLoadFailed')}</h1>
        <p>{metadataQuery.error?.message ?? t('templates.lookupDataMissing')}</p>
      </section>
    )
  }

  return (
    <PreventiveTemplateForm
      eyebrow={t('templates.detail.administration')}
      title={t('templates.create.title')}
      description={t('templates.create.description')}
      submitLabel={t('templates.save')}
      initialValues={createDefaultPreventiveTemplateFormValues(metadataQuery.data)}
      entityTypeOptions={metadataQuery.data.entityTypes}
      onSubmit={async (values) => {
        await createMutation.mutateAsync(values)
      }}
    />
  )
}
