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

export function PreventiveTemplateEditPage() {
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
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Preventive template could not be resolved.</h1>
        <p>The edit route requires a valid preventive template id and an authenticated session.</p>
      </section>
    )
  }

  if (templateQuery.isLoading || metadataQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Loading preventive template definition.</h1>
        <p>InfraOps is fetching the current template structure before editing.</p>
      </section>
    )
  }

  if (templateQuery.isError || !templateQuery.data || metadataQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Preventive template could not be loaded.</h1>
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
      eyebrow="Preventive template administration"
      title={`Edit ${template.name}`}
      description="Update metadata, sections, and checklist items through one aggregate edit flow."
      submitLabel="Save changes"
      initialValues={mapPreventiveTemplateToFormValues(template)}
      entityTypeOptions={entityTypeOptions}
      isEntityTypeLocked
      onSubmit={async (values) => {
        await updateMutation.mutateAsync(values)
      }}
    />
  )
}
