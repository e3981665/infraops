import { useEffect, useMemo, useState } from 'react'
import { useForm, useWatch, type Control, type UseFormRegister } from 'react-hook-form'
import { ApiError } from '@/shared/api/http-client'
import type {
  PreventiveExecutionAnswer,
  PreventiveExecutionAnswerInput,
  PreventiveExecutionTemplateItem,
  PreventiveExecutionTemplateSection,
} from '@/modules/preventive-executions/types/preventive-execution'
import { useTranslation } from '@/shared/i18n/useTranslation'
import type { SupportedLocale, TranslationKey } from '@/shared/i18n/translations'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

interface PreventiveExecutionFormValues {
  answers: Record<string, { value: string; comment: string }>
}

interface PreventiveExecutionFormProps {
  sections: PreventiveExecutionTemplateSection[]
  initialAnswers?: PreventiveExecutionAnswer[]
  isReadOnly?: boolean
  onSaveDraft?: (answers: PreventiveExecutionAnswerInput[]) => Promise<void>
  onSubmitExecution?: (answers: PreventiveExecutionAnswerInput[]) => Promise<void>
}

const emptyInitialAnswers: PreventiveExecutionAnswer[] = []

export function PreventiveExecutionForm({
  sections,
  initialAnswers = emptyInitialAnswers,
  isReadOnly = false,
  onSaveDraft,
  onSubmitExecution,
}: PreventiveExecutionFormProps) {
  const { locale, t } = useTranslation()
  const [submitError, setSubmitError] = useState<string | null>(null)
  const initialValues = useMemo(() => buildInitialValues(sections, initialAnswers), [
    initialAnswers,
    sections,
  ])
  const {
    control,
    getValues,
    register,
    reset,
    setError,
    clearErrors,
    formState: { errors, isSubmitting },
  } = useForm<PreventiveExecutionFormValues>({
    defaultValues: initialValues,
  })

  useEffect(() => {
    reset(initialValues)
  }, [initialValues, reset])

  async function handleSaveDraft() {
    setSubmitError(null)
    clearErrors()

    try {
      await onSaveDraft?.(mapAnswers(getValues()))
    } catch (error) {
      setSubmitError(getErrorMessage(error, t('executionForm.draftSaveFailed')))
    }
  }

  async function handleSubmitExecution() {
    setSubmitError(null)
    clearErrors()

    if (!validateRequiredAnswers(sections, getValues(), setError, t)) {
      setSubmitError(t('executionForm.requiredBeforeSubmit'))
      return
    }

    try {
      await onSubmitExecution?.(mapAnswers(getValues()))
    } catch (error) {
      setSubmitError(getErrorMessage(error, t('executionForm.submitFailed')))
    }
  }

  return (
    <form className="entity-type-form" noValidate>
      <div className="dynamic-field-list">
        {[...sections]
          .sort((left, right) => left.displayOrder - right.displayOrder)
          .map((section) => (
            <section className="dynamic-field-card" key={section.id}>
              <div className="form-section__heading">
                <div>
                  <h2>{localizeDemoText(section.title, locale)}</h2>
                </div>
              </div>

              <div className="execution-item-list">
                {[...section.checklistItems]
                  .sort((left, right) => left.displayOrder - right.displayOrder)
                  .map((item) => (
                    <PreventiveExecutionItemField
                      control={control}
                      error={errors.answers?.[item.itemKey]?.value?.message}
                      isReadOnly={isReadOnly}
                      item={item}
                      key={item.itemKey}
                      register={register}
                    />
                  ))}
              </div>
            </section>
          ))}
      </div>

      {submitError ? <p className="form-error">{submitError}</p> : null}

      {!isReadOnly ? (
        <div className="form-actions">
          <button
            className="button--secondary"
            disabled={isSubmitting}
            type="button"
            onClick={() => void handleSaveDraft()}
          >
            {t('executionForm.saveDraft')}
          </button>
          <button
            className="button"
            disabled={isSubmitting}
            type="button"
            onClick={() => void handleSubmitExecution()}
          >
            {t('common.submit')}
          </button>
        </div>
      ) : null}
    </form>
  )
}

interface PreventiveExecutionItemFieldProps {
  control: Control<PreventiveExecutionFormValues>
  error?: string
  isReadOnly: boolean
  item: PreventiveExecutionTemplateItem
  register: UseFormRegister<PreventiveExecutionFormValues>
}

function PreventiveExecutionItemField({
  control,
  error,
  isReadOnly,
  item,
  register,
}: PreventiveExecutionItemFieldProps) {
  const { locale, t } = useTranslation()
  const value = useWatch({
    control,
    name: `answers.${item.itemKey}.value`,
  })
  const comment = useWatch({
    control,
    name: `answers.${item.itemKey}.comment`,
  })
  const [isCommentOpen, setIsCommentOpen] = useState(false)
  const hasExistingComment = Boolean(comment?.trim())
  const showFailureComment =
    item.requiresCommentOnFailure &&
    item.itemType === 'yesNo' &&
    (value === 'false' || value === 'no')
  const showEditableComment = showFailureComment || isCommentOpen || hasExistingComment

  return (
    <div className="field execution-item">
      <label htmlFor={`execution-${item.itemKey}`}>
        {localizeDemoText(item.label, locale)}
        {item.isRequired ? <span aria-label={t('common.required')}> *</span> : null}
      </label>

      {renderAnswerInput(item, isReadOnly, register, t, locale)}

      {item.helpText ? (
        <span className="field__hint">{localizeDemoText(item.helpText, locale)}</span>
      ) : null}
      {item.itemType === 'numeric' && (item.minimumValue !== null || item.maximumValue !== null) ? (
        <span className="field__hint">
          {t('executionForm.expectedRange', {
            min: item.minimumValue ?? t('common.any'),
            max: item.maximumValue ?? t('common.any'),
          })}
        </span>
      ) : null}
      {error ? <span className="field__error">{error}</span> : null}

      {!isReadOnly && !showEditableComment ? (
        <button
          className="button--secondary execution-item__comment-toggle"
          type="button"
          onClick={() => setIsCommentOpen(true)}
        >
          {t('executionForm.addComment')}
        </button>
      ) : null}

      {showEditableComment && !isReadOnly ? (
        <div className="field">
          <label htmlFor={`execution-${item.itemKey}-comment`}>
            {t('common.comment')}
            {showFailureComment ? <span aria-label={t('common.required')}> *</span> : null}
          </label>
          <textarea
            id={`execution-${item.itemKey}-comment`}
            placeholder={t('executionForm.commentPlaceholder')}
            {...register(`answers.${item.itemKey}.comment`)}
          />
        </div>
      ) : null}

      {isReadOnly ? (
        <div className="field">
          <label htmlFor={`execution-${item.itemKey}-comment`}>{t('common.comment')}</label>
          <textarea
            id={`execution-${item.itemKey}-comment`}
            readOnly
            {...register(`answers.${item.itemKey}.comment`)}
          />
        </div>
      ) : null}
    </div>
  )
}

function renderAnswerInput(
  item: PreventiveExecutionTemplateItem,
  isReadOnly: boolean,
  register: UseFormRegister<PreventiveExecutionFormValues>,
  t: (key: TranslationKey, values?: Record<string, string | number>) => string,
  locale: SupportedLocale,
) {
  const fieldName = `answers.${item.itemKey}.value` as const
  const id = `execution-${item.itemKey}`

  if (item.itemType === 'yesNo') {
    return (
      <select id={id} disabled={isReadOnly} {...register(fieldName)}>
        <option value="">{t('common.choose')}</option>
        <option value="true">{t('common.yes')}</option>
        <option value="false">{t('common.no')}</option>
      </select>
    )
  }

  if (item.itemType === 'numeric') {
    return (
      <input
        id={id}
        readOnly={isReadOnly}
        step="any"
        type="number"
        {...register(fieldName)}
      />
    )
  }

  if (item.itemType === 'select') {
    return (
      <select id={id} disabled={isReadOnly} {...register(fieldName)}>
        <option value="">{t('common.choose')}</option>
        {[...item.options]
          .sort((left, right) => left.displayOrder - right.displayOrder)
          .map((option) => (
            <option key={option.id} value={option.value}>
              {localizeDemoText(option.label, locale)}
            </option>
          ))}
      </select>
    )
  }

  return <textarea id={id} readOnly={isReadOnly} {...register(fieldName)} />
}

function buildInitialValues(
  sections: PreventiveExecutionTemplateSection[],
  answers: PreventiveExecutionAnswer[],
): PreventiveExecutionFormValues {
  const answersByKey = new Map(answers.map((answer) => [answer.itemKey, answer]))
  const values: PreventiveExecutionFormValues = { answers: {} }

  sections
    .flatMap((section) => section.checklistItems)
    .forEach((item) => {
      const answer = answersByKey.get(item.itemKey)
      values.answers[item.itemKey] = {
        value: answer?.value ?? '',
        comment: answer?.comment ?? '',
      }
    })

  return values
}

function mapAnswers(values: PreventiveExecutionFormValues): PreventiveExecutionAnswerInput[] {
  return Object.entries(values.answers)
    .filter(([, answer]) => answer.value.trim() || answer.comment.trim())
    .map(([itemKey, answer]) => ({
      itemKey,
      value: answer.value.trim() || null,
      comment: answer.comment.trim() || null,
    }))
}

function validateRequiredAnswers(
  sections: PreventiveExecutionTemplateSection[],
  values: PreventiveExecutionFormValues,
  setError: ReturnType<typeof useForm<PreventiveExecutionFormValues>>['setError'],
  t: (key: TranslationKey, values?: Record<string, string | number>) => string,
) {
  let isValid = true

  sections
    .flatMap((section) => section.checklistItems)
    .forEach((item) => {
      const answer = values.answers[item.itemKey]

      if (item.isRequired && !answer?.value?.trim()) {
        setError(`answers.${item.itemKey}.value`, {
          type: 'required',
          message: t('executionForm.requiredItem'),
        })
        isValid = false
      }

      if (
        item.requiresCommentOnFailure &&
        item.itemType === 'yesNo' &&
        answer?.value === 'false' &&
        !answer.comment.trim()
      ) {
        setError(`answers.${item.itemKey}.value`, {
          type: 'required',
          message: t('executionForm.commentRequiredOnFailure'),
        })
        isValid = false
      }
    })

  return isValid
}

function getErrorMessage(error: unknown, fallback: string) {
  if (error instanceof ApiError) {
    return error.message
  }

  return fallback
}
