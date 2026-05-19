import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { AuditMetadata } from '@/components/audit/AuditMetadata'
import { StatusBadge } from '@/components/status/StatusBadge'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'
import { preventiveExecutionQueryKeys } from '@/modules/preventive-executions/api/preventive-execution-query-keys'
import { buildPreventiveExecutionEditPath, routePaths } from '@/shared/routing/route-paths'

export function PreventiveExecutionDetailPage() {
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
        <p className="hero-panel__eyebrow">Preventive execution</p>
        <h1>Loading execution.</h1>
      </section>
    )
  }

  if (executionQuery.isError || !executionQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive execution</p>
        <h1>Execution could not be loaded.</h1>
        <p>{executionQuery.error?.message}</p>
      </section>
    )
  }

  const execution = executionQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Execution detail</p>
          <h1>{execution.inventoryItemDisplayName}</h1>
        </div>
        <p>
          <StatusBadge value={execution.status} /> {execution.preventiveTemplateName}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.preventiveExecutions}>
          Back to executions
        </Link>
        {execution.status === 'draft' ? (
          <Link className="button" to={buildPreventiveExecutionEditPath(execution.id)}>
            Resume draft
          </Link>
        ) : null}
      </div>

      <dl className="definition-list definition-list--compact">
        <div>
          <dt>Entity type</dt>
          <dd>{execution.entityTypeName}</dd>
        </div>
        <div>
          <dt>Region</dt>
          <dd>{execution.regionName}</dd>
        </div>
        <div>
          <dt>Site</dt>
          <dd>{execution.siteName}</dd>
        </div>
        <div>
          <dt>Updated</dt>
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
        <h2>Validation history</h2>
        {execution.validationHistory.length === 0 ? (
          <p className="empty-copy">No validation action has been recorded for this execution.</p>
        ) : (
          <div className="timeline-list">
            {execution.validationHistory.map((record) => (
              <article className="timeline-item" key={record.id}>
                <StatusBadge value={record.actionType} />
                <p>{record.comment ?? 'No comment provided.'}</p>
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
