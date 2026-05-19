import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { ApiError } from '@/shared/api/http-client'

const actionSchema = z.object({
  approvalComment: z.string().max(2000).optional(),
  rejectionReason: z.string().max(2000).optional(),
  reworkReason: z.string().max(2000).optional(),
})

type ActionFormValues = z.infer<typeof actionSchema>

interface PreventiveValidationActionPanelProps {
  status: string
  onApprove: (comment: string | null) => Promise<void>
  onReject: (reason: string) => Promise<void>
  onRequestRework: (reason: string) => Promise<void>
}

export function PreventiveValidationActionPanel({
  status,
  onApprove,
  onReject,
  onRequestRework,
}: PreventiveValidationActionPanelProps) {
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)
  const {
    formState: { errors, isSubmitting },
    getValues,
    register,
    setError,
  } = useForm<ActionFormValues>({
    resolver: zodResolver(actionSchema),
    defaultValues: {
      approvalComment: '',
      rejectionReason: '',
      reworkReason: '',
    },
  })

  async function runAction(action: () => Promise<void>, successMessage: string) {
    setActionError(null)
    setActionSuccess(null)

    try {
      await action()
      setActionSuccess(successMessage)
    } catch (error) {
      setActionError(getErrorMessage(error, 'InfraOps could not complete the validation action.'))
    }
  }

  async function handleApprove() {
    const comment = getValues('approvalComment')?.trim() || null
    await runAction(() => onApprove(comment), 'Execution approved.')
  }

  async function handleReject() {
    const reason = getValues('rejectionReason')?.trim()

    if (!reason) {
      setError('rejectionReason', {
        type: 'required',
        message: 'Rejection reason is required.',
      })
      return
    }

    await runAction(() => onReject(reason), 'Execution rejected.')
  }

  async function handleRequestRework() {
    const reason = getValues('reworkReason')?.trim()

    if (!reason) {
      setError('reworkReason', {
        type: 'required',
        message: 'Rework reason is required.',
      })
      return
    }

    await runAction(() => onRequestRework(reason), 'Rework requested.')
  }

  if (status !== 'submitted') {
    return (
      <section className="form-section">
        <h2>Validation action</h2>
        <p>This execution has already moved out of the submitted queue.</p>
      </section>
    )
  }

  return (
    <section className="form-section">
      <h2>Validation action</h2>
      <form noValidate>
        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="approvalComment">Approval comment</label>
            <textarea id="approvalComment" {...register('approvalComment')} />
            {errors.approvalComment ? (
              <span className="field__error">{errors.approvalComment.message}</span>
            ) : null}
            <button
              className="button"
              disabled={isSubmitting}
              type="button"
              onClick={() => void handleApprove()}
            >
              Approve
            </button>
          </div>

          <div className="field">
            <label htmlFor="rejectionReason">Rejection reason</label>
            <textarea id="rejectionReason" {...register('rejectionReason')} />
            {errors.rejectionReason ? (
              <span className="field__error">{errors.rejectionReason.message}</span>
            ) : null}
            <button
              className="button--secondary"
              disabled={isSubmitting}
              type="button"
              onClick={() => void handleReject()}
            >
              Reject
            </button>
          </div>

          <div className="field">
            <label htmlFor="reworkReason">Rework reason</label>
            <textarea id="reworkReason" {...register('reworkReason')} />
            {errors.reworkReason ? (
              <span className="field__error">{errors.reworkReason.message}</span>
            ) : null}
            <button
              className="button--secondary"
              disabled={isSubmitting}
              type="button"
              onClick={() => void handleRequestRework()}
            >
              Request rework
            </button>
          </div>
        </div>

        {actionError ? <p className="form-error">{actionError}</p> : null}
        {actionSuccess ? <p className="form-success">{actionSuccess}</p> : null}
      </form>
    </section>
  )
}

function getErrorMessage(error: unknown, fallback: string) {
  if (error instanceof ApiError) {
    return error.message
  }

  return fallback
}
