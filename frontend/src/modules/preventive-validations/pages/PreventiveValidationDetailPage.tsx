import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { AuditMetadata } from '@/components/audit/AuditMetadata'
import { StatusBadge } from '@/components/status/StatusBadge'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import { PreventiveValidationActionPanel } from '@/modules/preventive-validations/components/PreventiveValidationActionPanel'
import { preventiveValidationQueryKeys } from '@/modules/preventive-validations/api/preventive-validation-query-keys'
import { preventiveValidationsClient } from '@/modules/preventive-validations/api/preventive-validations-client'
import { buildPreventiveValidationsPath } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

export function PreventiveValidationDetailPage() {
  const { locale, t } = useTranslation()
  const { preventiveExecutionId } = useParams()
  const navigate = useNavigate()
  const { session } = useAuthSession()
  const queryClient = useQueryClient()
  const accessToken = session?.tokens.accessToken

  const validationQuery = useQuery({
    queryKey: preventiveValidationQueryKeys.detail(preventiveExecutionId ?? ''),
    queryFn: () => preventiveValidationsClient.getById(preventiveExecutionId!, accessToken!),
    enabled: Boolean(accessToken && preventiveExecutionId),
  })

  const approveMutation = useMutation({
    mutationFn: (comment: string | null) =>
      preventiveValidationsClient.approve(preventiveExecutionId!, { comment }, accessToken!),
    onSuccess: async (updated) => {
      queryClient.setQueryData(preventiveValidationQueryKeys.detail(updated.id), updated)
      await queryClient.invalidateQueries({ queryKey: preventiveValidationQueryKeys.all })
    },
  })

  const rejectMutation = useMutation({
    mutationFn: (reason: string) =>
      preventiveValidationsClient.reject(preventiveExecutionId!, { reason }, accessToken!),
    onSuccess: async (updated) => {
      queryClient.setQueryData(preventiveValidationQueryKeys.detail(updated.id), updated)
      await queryClient.invalidateQueries({ queryKey: preventiveValidationQueryKeys.all })
    },
  })

  const reworkMutation = useMutation({
    mutationFn: (reason: string) =>
      preventiveValidationsClient.requestRework(preventiveExecutionId!, { reason }, accessToken!),
    onSuccess: async (updated) => {
      queryClient.setQueryData(preventiveValidationQueryKeys.detail(updated.id), updated)
      await queryClient.invalidateQueries({ queryKey: preventiveValidationQueryKeys.all })
    },
  })

  async function approveExecution(comment: string | null) {
    await approveMutation.mutateAsync(comment)
    navigate(buildPreventiveValidationsPath({ status: 'approved' }))
  }

  async function rejectExecution(reason: string) {
    await rejectMutation.mutateAsync(reason)
    navigate(buildPreventiveValidationsPath({ status: 'rejected' }))
  }

  async function requestExecutionRework(reason: string) {
    await reworkMutation.mutateAsync(reason)
    navigate(buildPreventiveValidationsPath({ status: 'reworkRequested' }))
  }

  if (validationQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('validations.eyebrow')}</p>
        <h1>{t('validations.loadingDetail')}</h1>
      </section>
    )
  }

  if (validationQuery.isError || !validationQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('validations.eyebrow')}</p>
        <h1>{t('validations.detailLoadFailed')}</h1>
        <p>{validationQuery.error?.message}</p>
      </section>
    )
  }

  const execution = validationQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('validations.reviewEyebrow')}</p>
          <h1>{execution.inventoryItemDisplayName}</h1>
        </div>
        <p>
          <StatusBadge value={execution.status} />{' '}
          {localizeDemoText(execution.preventiveTemplateName, locale)}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link
          className="button--secondary"
          to={buildPreventiveValidationsPath({ status: execution.status })}
        >
          {t('validations.backToQueue')}
        </Link>
      </div>

      <dl className="definition-list definition-list--compact">
        <div>
          <dt>{t('common.entityType')}</dt>
          <dd>{localizeDemoText(execution.entityTypeName, locale)}</dd>
        </div>
        <div>
          <dt>{t('common.region')}</dt>
          <dd>{localizeDemoText(execution.regionName, locale)}</dd>
        </div>
        <div>
          <dt>{t('common.site')}</dt>
          <dd>{localizeDemoText(execution.siteName, locale)}</dd>
        </div>
        <div>
          <dt>{t('validations.submittedBy')}</dt>
          <dd>{execution.submittedBy ?? t('validations.notSubmitted')}</dd>
        </div>
        <div>
          <dt>{t('validations.submittedAt')}</dt>
          <dd>
            {execution.submittedAtUtc
              ? new Date(execution.submittedAtUtc).toLocaleString()
              : t('validations.notSubmitted')}
          </dd>
        </div>
      </dl>

      <AuditMetadata
        createdAtUtc={execution.createdAtUtc}
        createdBy={execution.createdBy}
        submittedAtUtc={execution.submittedAtUtc}
        submittedBy={execution.submittedBy}
        updatedAtUtc={execution.updatedAtUtc}
        updatedBy={execution.updatedBy}
      />

      <PreventiveValidationActionPanel
        status={execution.status}
        onApprove={approveExecution}
        onReject={rejectExecution}
        onRequestRework={requestExecutionRework}
      />

      <section className="form-section">
        <h2>{t('executions.validationHistory')}</h2>
        {execution.validationHistory.length === 0 ? (
          <p>{t('validations.noHistory')}</p>
        ) : (
          <div className="table-panel">
            <table className="data-table">
              <thead>
                <tr>
                  <th scope="col">{t('common.action')}</th>
                  <th scope="col">{t('common.validator')}</th>
                  <th scope="col">{t('common.when')}</th>
                  <th scope="col">{t('common.comment')}</th>
                </tr>
              </thead>
              <tbody>
                {execution.validationHistory.map((record) => (
                  <tr key={record.id}>
                    <td>
                      <StatusBadge value={record.actionType} />
                    </td>
                    <td>{record.validatorUserId}</td>
                    <td>{new Date(record.createdAtUtc).toLocaleString()}</td>
                    <td>{localizeDemoText(record.comment, locale)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <PreventiveExecutionForm
        initialAnswers={execution.answers}
        isReadOnly
        sections={execution.templateSections}
      />
    </section>
  )
}
