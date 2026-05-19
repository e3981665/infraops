import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { AuditMetadata } from '@/components/audit/AuditMetadata'
import { StatusBadge } from '@/components/status/StatusBadge'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'
import { preventiveExecutionQueryKeys } from '@/modules/preventive-executions/api/preventive-execution-query-keys'
import { buildPreventiveExecutionEditPath, routePaths } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

export function PreventiveExecutionDetailPage() {
  const { locale, t } = useTranslation()
  const { preventiveExecutionId } = useParams()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const executionQuery = useQuery({
    queryKey: preventiveExecutionQueryKeys.detail(preventiveExecutionId ?? ''),
    queryFn: () => preventiveExecutionsClient.getById(preventiveExecutionId!, accessToken!),
    enabled: Boolean(accessToken && preventiveExecutionId),
  })

  if (executionQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('executions.eyebrow')}</p>
        <h1>{t('executions.loadingDetail')}</h1>
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
          <p className="hero-panel__eyebrow">{t('executions.detailEyebrow')}</p>
          <h1>{execution.inventoryItemDisplayName}</h1>
        </div>
        <p>
          <StatusBadge value={execution.status} />{' '}
          {localizeDemoText(execution.preventiveTemplateName, locale)}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.preventiveExecutions}>
          {t('executions.backToExecutions')}
        </Link>
        {execution.status === 'draft' ? (
          <Link className="button" to={buildPreventiveExecutionEditPath(execution.id)}>
            {t('executions.resumeDraft')}
          </Link>
        ) : null}
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
          <dt>{t('common.updated')}</dt>
          <dd>{new Date(execution.updatedAtUtc).toLocaleString()}</dd>
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

      <section className="form-section">
        <h2>{t('executions.validationHistory')}</h2>
        {execution.validationHistory.length === 0 ? (
          <p className="empty-copy">{t('executions.noValidationHistory')}</p>
        ) : (
          <div className="timeline-list">
            {execution.validationHistory.map((record) => (
              <article className="timeline-item" key={record.id}>
                <StatusBadge value={record.actionType} />
                <p>{localizeDemoText(record.comment, locale) || t('executions.noComment')}</p>
                <small>
                  {record.validatorUserId} - {new Date(record.createdAtUtc).toLocaleString()}
                </small>
              </article>
            ))}
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
