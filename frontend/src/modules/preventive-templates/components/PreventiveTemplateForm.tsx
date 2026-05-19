import { useEffect, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  useFieldArray,
  useForm,
  type FieldErrors,
  type Path,
  type UseFormSetError,
} from 'react-hook-form'
import { ApiError } from '@/shared/api/http-client'
import {
  preventiveTemplateFormSchema,
  type PreventiveTemplateFormValues,
} from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import type { PreventiveTemplateEntityTypeOption } from '@/modules/preventive-templates/types/preventive-template'
import { PreventiveTemplateSectionEditor } from '@/modules/preventive-templates/components/PreventiveTemplateSectionEditor'
import {
  createDefaultSection,
  normalizePreventiveTemplateCode,
} from '@/modules/preventive-templates/utils/preventive-template-form-utils'

interface PreventiveTemplateFormProps {
  eyebrow: string
  title: string
  description: string
  submitLabel: string
  initialValues: PreventiveTemplateFormValues
  entityTypeOptions: PreventiveTemplateEntityTypeOption[]
  isEntityTypeLocked?: boolean
  onSubmit: (values: PreventiveTemplateFormValues) => Promise<void>
}

export function PreventiveTemplateForm({
  eyebrow,
  title,
  description,
  submitLabel,
  initialValues,
  entityTypeOptions,
  isEntityTypeLocked = false,
  onSubmit,
}: PreventiveTemplateFormProps) {
  const [submitError, setSubmitError] = useState<string | null>(null)
  const {
    control,
    handleSubmit,
    register,
    reset,
    setError,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<PreventiveTemplateFormValues>({
    resolver: zodResolver(preventiveTemplateFormSchema),
    defaultValues: initialValues,
  })

  const sections = useFieldArray({
    control,
    name: 'sections',
  })

  useEffect(() => {
    reset(initialValues)
  }, [initialValues, reset])

  async function handleFormSubmit(values: PreventiveTemplateFormValues) {
    setSubmitError(null)

    try {
      await onSubmit(values)
    } catch (error) {
      if (error instanceof ApiError) {
        const handled = applyApiValidationErrors(error, setError)
        setSubmitError(handled ? null : error.message)
        return
      }

      setSubmitError('InfraOps could not save the preventive template.')
    }
  }

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{eyebrow}</p>
          <h1>{title}</h1>
        </div>
        <p>{description}</p>
      </div>

      <form className="entity-type-form" onSubmit={handleSubmit(handleFormSubmit)} noValidate>
        <section className="form-section">
          <div className="form-section__heading">
            <div>
              <h2>Template metadata</h2>
              <p>Define the stable template reference and the entity type it belongs to.</p>
            </div>
          </div>

          <div className="field-grid field-grid--three-columns">
            <div className="field">
              <label htmlFor="preventiveTemplateEntityTypeId">Entity type</label>
              <select
                id="preventiveTemplateEntityTypeId"
                disabled={isEntityTypeLocked}
                {...register('entityTypeId')}
              >
                <option value="">Select an entity type</option>
                {entityTypeOptions.map((entityType) => (
                  <option key={entityType.id} value={entityType.id}>
                    {entityType.name}
                  </option>
                ))}
              </select>
              {errors.entityTypeId ? (
                <span className="field__error">{errors.entityTypeId.message}</span>
              ) : null}
            </div>

            <div className="field">
              <label htmlFor="preventiveTemplateName">Template name</label>
              <input id="preventiveTemplateName" type="text" {...register('name')} />
              {errors.name ? <span className="field__error">{errors.name.message}</span> : null}
            </div>

            <div className="field">
              <label htmlFor="preventiveTemplateCode">Template code</label>
              <input
                id="preventiveTemplateCode"
                type="text"
                {...register('code', {
                  onBlur: (event) => {
                    setValue('code', normalizePreventiveTemplateCode(event.target.value), {
                      shouldValidate: true,
                    })
                  },
                })}
              />
              {errors.code ? <span className="field__error">{errors.code.message}</span> : null}
            </div>
          </div>

          <div className="field">
            <label htmlFor="preventiveTemplateDescription">Description</label>
            <textarea id="preventiveTemplateDescription" rows={4} {...register('description')} />
            {errors.description ? (
              <span className="field__error">{errors.description.message}</span>
            ) : null}
          </div>

          {entityTypeOptions.length === 0 ? (
            <div className="empty-state">
              <h2>No active entity types are available.</h2>
              <p>Create or reactivate an entity type before defining preventive templates.</p>
            </div>
          ) : null}
        </section>

        <section className="form-section">
          <div className="form-section__heading">
            <div>
              <h2>Template structure</h2>
              <p>Sections and checklist items stay inside one aggregate update flow.</p>
            </div>
            <button
              className="button--secondary"
              type="button"
              onClick={() => sections.append(createDefaultSection(sections.fields.length + 1))}
            >
              Add section
            </button>
          </div>

          <div className="entity-field-list">
            {sections.fields.map((section, sectionIndex) => (
              <PreventiveTemplateSectionEditor
                key={section.id}
                control={control}
                register={register}
                setValue={setValue}
                sectionIndex={sectionIndex}
                onRemoveSection={() => sections.remove(sectionIndex)}
                error={errors.sections?.[sectionIndex] as FieldErrors<PreventiveTemplateFormValues['sections'][number]> | undefined}
                isPersisted={Boolean(section.id)}
              />
            ))}
          </div>

          {'message' in (errors.sections ?? {}) && errors.sections?.message ? (
            <span className="field__error">{errors.sections.message}</span>
          ) : null}
        </section>

        {submitError ? <p className="form-error">{submitError}</p> : null}

        <div className="form-actions">
          <button className="button" type="submit" disabled={isSubmitting || entityTypeOptions.length === 0}>
            {isSubmitting ? 'Saving...' : submitLabel}
          </button>
        </div>
      </form>
    </section>
  )
}

function applyApiValidationErrors(
  error: ApiError,
  setError: UseFormSetError<PreventiveTemplateFormValues>,
) {
  if (!error.errors) {
    return false
  }

  let handled = false

  Object.entries(error.errors).forEach(([propertyName, messages]) => {
    const message = messages[0] ?? error.message
    const formPath = toFormPath(propertyName)

    if (!formPath) {
      return
    }

    setError(formPath as Path<PreventiveTemplateFormValues>, {
      type: 'server',
      message,
    })
    handled = true
  })

  return handled
}

function toFormPath(propertyName: string) {
  if (!propertyName) {
    return null
  }

  return propertyName
    .replace(/([A-Za-z]+)\[(\d+)\]/g, (_, collection, index: string) => `${lowercaseFirst(collection)}.${index}`)
    .split('.')
    .map((segment) => (segment && Number.isNaN(Number(segment)) ? lowercaseFirst(segment) : segment))
    .join('.')
}

function lowercaseFirst(value: string) {
  return value.length > 1 ? `${value[0]!.toLowerCase()}${value.slice(1)}` : value.toLowerCase()
}
