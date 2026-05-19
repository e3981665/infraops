import { useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { ApiError } from '@/shared/api/http-client'
import { useTranslation } from '@/shared/i18n/useTranslation'

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
  const { t } = useTranslation()
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
      setActionError(getErrorMessage(error, t('validations.actionFailed')))
    }
  }

  async function handleApprove() {
    const comment = getValues('approvalComment')?.trim() || null
    await runAction(() => onApprove(comment), t('validations.approvedSuccess'))
  }

  async function handleReject() {
    const reason = getValues('rejectionReason')?.trim()

    if (!reason) {
      setError('rejectionReason', {
        type: 'required',
        message: t('validations.rejectionRequired'),
      })
      return
    }

    await runAction(() => onReject(reason), t('validations.rejectedSuccess'))
  }

  async function handleRequestRework() {
    const reason = getValues('reworkReason')?.trim()

    if (!reason) {
      setError('reworkReason', {
        type: 'required',
        message: t('validations.reworkRequired'),
      })
      return
    }

    await runAction(() => onRequestRework(reason), t('validations.reworkSuccess'))
  }

  if (status !== 'submitted') {
    return (
      <section className="form-section">
        <h2>{t('validations.actionPanelTitle')}</h2>
        <p>{t('validations.alreadyMoved')}</p>
      </section>
    )
  }

  return (
    <section className="form-section">
      <div className="form-section__heading">
        <div>
          <h2>{t('validations.actionPanelTitle')}</h2>
          <p>{t('validations.actionPanelHelp')}</p>
        </div>
      </div>
      <form noValidate>
        <div className="validation-action-grid">
          <article className="validation-action-card validation-action-card--approve">
            <div>
              <h3>{t('status.approve')}</h3>
              <p>{t('validations.approveHelp')}</p>
            </div>
            <div className="field">
            <label htmlFor="approvalComment">{t('validations.approvalComment')}</label>
            <textarea
              id="approvalComment"
              placeholder={t('validations.approvalPlaceholder')}
              {...register('approvalComment')}
            />
            {errors.approvalComment ? (
              <span className="field__error">{errors.approvalComment.message}</span>
            ) : null}
            </div>
            <button
              className="button"
              disabled={isSubmitting}
              type="button"
              onClick={() => void handleApprove()}
            >
              {t('status.approve')}
            </button>
          </article>

          <article className="validation-action-card validation-action-card--reject">
            <div>
              <h3>{t('status.reject')}</h3>
              <p>{t('validations.rejectHelp')}</p>
            </div>
            <div className="field">
            <label htmlFor="rejectionReason">{t('validations.rejectionReason')}</label>
            <textarea
              id="rejectionReason"
              placeholder={t('validations.reasonPlaceholder')}
              {...register('rejectionReason')}
            />
            {errors.rejectionReason ? (
              <span className="field__error">{errors.rejectionReason.message}</span>
            ) : null}
            </div>
            <button
              className="button--danger"
              disabled={isSubmitting}
              type="button"
              onClick={() => void handleReject()}
            >
              {t('status.reject')}
            </button>
          </article>

          <article className="validation-action-card validation-action-card--rework">
            <div>
              <h3>{t('status.requestRework')}</h3>
              <p>{t('validations.reworkHelp')}</p>
            </div>
            <div className="field">
            <label htmlFor="reworkReason">{t('validations.reworkReason')}</label>
            <textarea
              id="reworkReason"
              placeholder={t('validations.reasonPlaceholder')}
              {...register('reworkReason')}
            />
            {errors.reworkReason ? (
              <span className="field__error">{errors.reworkReason.message}</span>
            ) : null}
            </div>
            <button
              className="button--secondary"
              disabled={isSubmitting}
              type="button"
              onClick={() => void handleRequestRework()}
            >
              {t('status.requestRework')}
            </button>
          </article>
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
