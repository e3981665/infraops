import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { AuditMetadata } from '@/components/audit/AuditMetadata'
import { StatusBadge } from '@/components/status/StatusBadge'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import { PreventiveValidationActionPanel } from '@/modules/preventive-validations/components/PreventiveValidationActionPanel'
import { preventiveValidationQueryKeys } from '@/modules/preventive-validations/api/preventive-validation-query-keys'
import { preventiveValidationsClient } from '@/modules/preventive-validations/api/preventive-validations-client'
import { routePaths } from '@/shared/routing/route-paths'

export function PreventiveValidationDetailPage() {
  const { preventiveExecutionId } = useParams()
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
    onSuccess: (updated) => {
      queryClient.setQueryData(preventiveValidationQueryKeys.detail(updated.id), updated)
      void queryClient.invalidateQueries({ queryKey: preventiveValidationQueryKeys.all })
    },
  })

  const rejectMutation = useMutation({
    mutationFn: (reason: string) =>
      preventiveValidationsClient.reject(preventiveExecutionId!, { reason }, accessToken!),
    onSuccess: (updated) => {
      queryClient.setQueryData(preventiveValidationQueryKeys.detail(updated.id), updated)
      void queryClient.invalidateQueries({ queryKey: preventiveValidationQueryKeys.all })
    },
  })

  const reworkMutation = useMutation({
    mutationFn: (reason: string) =>
      preventiveValidationsClient.requestRework(preventiveExecutionId!, { reason }, accessToken!),
    onSuccess: (updated) => {
      queryClient.setQueryData(preventiveValidationQueryKeys.detail(updated.id), updated)
      void queryClient.invalidateQueries({ queryKey: preventiveValidationQueryKeys.all })
    },
  })

  if (validationQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive validation</p>
        <h1>Loading validation detail.</h1>
      </section>
    )
  }

  if (validationQuery.isError || !validationQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive validation</p>
        <h1>Validation detail could not be loaded.</h1>
        <p>{validationQuery.error?.message}</p>
      </section>
    )
  }

  const execution = validationQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Validation review</p>
          <h1>{execution.inventoryItemDisplayName}</h1>
        </div>
        <p>
          <StatusBadge value={execution.status} /> {execution.preventiveTemplateName}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.preventiveValidations}>
          Back to validation queue
        </Link>
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
          <dt>Submitted by</dt>
          <dd>{execution.submittedBy ?? 'Not submitted'}</dd>
        </div>
        <div>
          <dt>Submitted at</dt>
          <dd>
            {execution.submittedAtUtc
              ? new Date(execution.submittedAtUtc).toLocaleString()
              : 'Not submitted'}
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
        onApprove={(comment) => approveMutation.mutateAsync(comment).then(() => undefined)}
        onReject={(reason) => rejectMutation.mutateAsync(reason).then(() => undefined)}
        onRequestRework={(reason) => reworkMutation.mutateAsync(reason).then(() => undefined)}
      />

      <section className="form-section">
        <h2>Validation history</h2>
        {execution.validationHistory.length === 0 ? (
          <p>No validation actions have been recorded.</p>
        ) : (
          <div className="table-panel">
            <table className="data-table">
              <thead>
                <tr>
                  <th scope="col">Action</th>
                  <th scope="col">Validator</th>
                  <th scope="col">When</th>
                  <th scope="col">Comment</th>
                </tr>
              </thead>
              <tbody>
                {execution.validationHistory.map((record) => (
                  <tr key={record.id}>
                    <td>{record.actionType}</td>
                    <td>{record.validatorUserId}</td>
                    <td>{new Date(record.createdAtUtc).toLocaleString()}</td>
                    <td>{record.comment ?? ''}</td>
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
