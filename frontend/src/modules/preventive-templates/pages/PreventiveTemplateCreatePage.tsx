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

export function PreventiveTemplateCreatePage() {
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
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Authenticated access is required.</h1>
        <p>The admin workspace is waiting for a valid session.</p>
      </section>
    )
  }

  if (metadataQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Loading preventive template metadata.</h1>
        <p>InfraOps is preparing the template definition form.</p>
      </section>
    )
  }

  if (metadataQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Preventive template metadata could not be loaded.</h1>
        <p>{metadataQuery.error?.message ?? 'The required lookup data was not found.'}</p>
      </section>
    )
  }

  return (
    <PreventiveTemplateForm
      eyebrow="Preventive template administration"
      title="Create a preventive template"
      description="Define the entity-type checklist structure that future execution flows will render."
      submitLabel="Save preventive template"
      initialValues={createDefaultPreventiveTemplateFormValues(metadataQuery.data)}
      entityTypeOptions={metadataQuery.data.entityTypes}
      onSubmit={async (values) => {
        await createMutation.mutateAsync(values)
      }}
    />
  )
}
