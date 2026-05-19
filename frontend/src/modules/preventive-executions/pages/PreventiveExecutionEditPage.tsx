import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'
import { preventiveExecutionQueryKeys } from '@/modules/preventive-executions/api/preventive-execution-query-keys'
import type { PreventiveExecutionAnswerInput } from '@/modules/preventive-executions/types/preventive-execution'
import { buildPreventiveExecutionDetailPath, routePaths } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

export function PreventiveExecutionEditPage() {
  const { locale, t } = useTranslation()
  const { preventiveExecutionId } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const executionQuery = useQuery({
    queryKey: preventiveExecutionQueryKeys.detail(preventiveExecutionId ?? ''),
    queryFn: () => preventiveExecutionsClient.getById(preventiveExecutionId!, accessToken!),
    enabled: Boolean(accessToken && preventiveExecutionId),
  })

  const saveDraftMutation = useMutation({
    mutationFn: (answers: PreventiveExecutionAnswerInput[]) =>
      preventiveExecutionsClient.saveDraft(preventiveExecutionId!, answers, accessToken!),
    onSuccess: async (execution) => {
      await queryClient.invalidateQueries({
        queryKey: preventiveExecutionQueryKeys.detail(execution.id),
      })
    },
  })

  const submitMutation = useMutation({
    mutationFn: (answers: PreventiveExecutionAnswerInput[]) =>
      preventiveExecutionsClient.submit(preventiveExecutionId!, answers, accessToken!),
    onSuccess: async (execution) => {
      await queryClient.invalidateQueries({
        queryKey: preventiveExecutionQueryKeys.detail(execution.id),
      })
      navigate(buildPreventiveExecutionDetailPath(execution.id))
    },
  })

  if (executionQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
        <h1>{t('executions.loadingDraft')}</h1>
      </section>
    )
  }

  if (executionQuery.isError || !executionQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
        <h1>{t('executions.loadFailed')}</h1>
        <p>{executionQuery.error?.message}</p>
      </section>
    )
  }

  const execution = executionQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('executions.draftEyebrow')}</p>
          <h1>{execution.inventoryItemDisplayName}</h1>
        </div>
        <p>
          {t('executions.templateForEntityType', {
            template: localizeDemoText(execution.preventiveTemplateName, locale),
            entityType: localizeDemoText(execution.entityTypeName, locale),
          })}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.preventiveExecutions}>
          {t('executions.backToExecutions')}
        </Link>
      </div>

      <PreventiveExecutionForm
        initialAnswers={execution.answers}
        isReadOnly={execution.status !== 'draft'}
        sections={execution.templateSections}
        onSaveDraft={async (answers) => {
          await saveDraftMutation.mutateAsync(answers)
        }}
        onSubmitExecution={async (answers) => {
          await submitMutation.mutateAsync(answers)
        }}
      />
    </section>
  )
}
