import { useEffect, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  useFieldArray,
  useForm,
  useWatch,
  type Control,
  type FieldErrors,
  type UseFormRegister,
  type UseFormSetValue,
} from 'react-hook-form'
import { ApiError } from '@/shared/api/http-client'
import {
  entityTypeFormSchema,
  type EntityTypeFormValues,
} from '@/modules/entity-types/schemas/entity-type-form-schema'
import {
  createDefaultFieldDefinition,
  createDefaultFieldOption,
  normalizeEntityTypeCode,
  normalizeFieldKey,
} from '@/modules/entity-types/utils/entity-type-form-utils'

interface EntityTypeFormProps {
  eyebrow: string
  title: string
  description: string
  submitLabel: string
  initialValues: EntityTypeFormValues
  onSubmit: (values: EntityTypeFormValues) => Promise<void>
}

interface FieldDefinitionEditorProps {
  control: Control<EntityTypeFormValues>
  register: UseFormRegister<EntityTypeFormValues>
  setValue: UseFormSetValue<EntityTypeFormValues>
  index: number
  onRemoveField: () => void
  error?: FieldErrors<EntityTypeFormValues['fieldDefinitions'][number]>
  isPersisted: boolean
}

export function EntityTypeForm({
  eyebrow,
  title,
  description,
  submitLabel,
  initialValues,
  onSubmit,
}: EntityTypeFormProps) {
  const [submitError, setSubmitError] = useState<string | null>(null)
  const {
    control,
    handleSubmit,
    register,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<EntityTypeFormValues>({
    resolver: zodResolver(entityTypeFormSchema),
    defaultValues: initialValues,
  })

  const fieldDefinitions = useFieldArray({
    control,
    name: 'fieldDefinitions',
  })

  useEffect(() => {
    reset(initialValues)
  }, [initialValues, reset])

  async function handleFormSubmit(values: EntityTypeFormValues) {
    setSubmitError(null)

    try {
      await onSubmit(values)
    } catch (error) {
      if (error instanceof ApiError) {
        setSubmitError(error.message)
        return
      }

      setSubmitError('InfraOps could not save the entity type definition.')
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
              <h2>Definition metadata</h2>
              <p>Name, stable code, and context for the admin-facing definition.</p>
            </div>
          </div>

          <div className="field-grid field-grid--two-columns">
            <div className="field">
              <label htmlFor="entityTypeName">Entity type name</label>
              <input id="entityTypeName" type="text" {...register('name')} />
              {errors.name ? <span className="field__error">{errors.name.message}</span> : null}
            </div>

            <div className="field">
              <label htmlFor="entityTypeCode">Entity type code</label>
              <input
                id="entityTypeCode"
                type="text"
                {...register('code', {
                  onBlur: (event) => {
                    setValue('code', normalizeEntityTypeCode(event.target.value), {
                      shouldValidate: true,
                    })
                  },
                })}
              />
              {errors.code ? <span className="field__error">{errors.code.message}</span> : null}
            </div>
          </div>

          <div className="field">
            <label htmlFor="entityTypeDescription">Description</label>
            <textarea id="entityTypeDescription" rows={4} {...register('description')} />
            {errors.description ? (
              <span className="field__error">{errors.description.message}</span>
            ) : null}
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__heading">
            <div>
              <h2>Dynamic field definitions</h2>
              <p>
                These fields define what Inventory will later render and validate
                for this entity type.
              </p>
            </div>
            <button
              className="button--secondary"
              type="button"
              onClick={() => {
                fieldDefinitions.append(
                  createDefaultFieldDefinition(fieldDefinitions.fields.length + 1),
                )
              }}
            >
              Add field
            </button>
          </div>

          <div className="entity-field-list">
            {fieldDefinitions.fields.map((fieldDefinition, index) => (
              <FieldDefinitionEditor
                key={fieldDefinition.id}
                control={control}
                register={register}
                setValue={setValue}
                index={index}
                onRemoveField={() => fieldDefinitions.remove(index)}
                error={
                  errors.fieldDefinitions?.[index] as
                    | FieldErrors<EntityTypeFormValues['fieldDefinitions'][number]>
                    | undefined
                }
                isPersisted={Boolean(fieldDefinition.id)}
              />
            ))}
          </div>
        </section>

        {submitError ? <p className="form-error">{submitError}</p> : null}

        <div className="form-actions">
          <button className="button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : submitLabel}
          </button>
        </div>
      </form>
    </section>
  )
}

function FieldDefinitionEditor({
  control,
  register,
  setValue,
  index,
  onRemoveField,
  error,
  isPersisted,
}: FieldDefinitionEditorProps) {
  const fieldType = useWatch({
    control,
    name: `fieldDefinitions.${index}.fieldType`,
  })
  const isActive = useWatch({
    control,
    name: `fieldDefinitions.${index}.isActive`,
  })
  const options = useFieldArray({
    control,
    name: `fieldDefinitions.${index}.options`,
  })

  useEffect(() => {
    if (fieldType !== 'select' && options.fields.length > 0) {
      options.replace([])
    }
  }, [fieldType, options])

  return (
    <article
      className={`entity-field-editor${isActive ? '' : ' entity-field-editor--inactive'}`}
    >
      <div className="entity-field-editor__header">
        <div>
          <h3>Field {index + 1}</h3>
          <p>{isActive ? 'Active definition' : 'Inactive definition kept for history.'}</p>
        </div>
        <div className="button-row">
          {isPersisted ? (
            <button
              className="button--secondary"
              type="button"
              onClick={() => {
                setValue(`fieldDefinitions.${index}.isActive`, !isActive, {
                  shouldValidate: true,
                })
              }}
            >
              {isActive ? 'Deactivate field' : 'Reactivate field'}
            </button>
          ) : (
            <button className="button--secondary" type="button" onClick={onRemoveField}>
              Remove field
            </button>
          )}
        </div>
      </div>

      <div className="field-grid field-grid--three-columns">
        <div className="field">
          <label htmlFor={`fieldDefinitions.${index}.fieldKey`}>Field key</label>
          <input
            id={`fieldDefinitions.${index}.fieldKey`}
            type="text"
            {...register(`fieldDefinitions.${index}.fieldKey`, {
              onBlur: (event) => {
                setValue(
                  `fieldDefinitions.${index}.fieldKey`,
                  normalizeFieldKey(event.target.value),
                  { shouldValidate: true },
                )
              },
            })}
          />
          {error?.fieldKey ? <span className="field__error">{error.fieldKey.message}</span> : null}
        </div>

        <div className="field">
          <label htmlFor={`fieldDefinitions.${index}.displayLabel`}>Display label</label>
          <input
            id={`fieldDefinitions.${index}.displayLabel`}
            type="text"
            {...register(`fieldDefinitions.${index}.displayLabel`)}
          />
          {error?.displayLabel ? (
            <span className="field__error">{error.displayLabel.message}</span>
          ) : null}
        </div>

        <div className="field">
          <label htmlFor={`fieldDefinitions.${index}.fieldType`}>Field type</label>
          <select id={`fieldDefinitions.${index}.fieldType`} {...register(`fieldDefinitions.${index}.fieldType`)}>
            <option value="text">Text</option>
            <option value="textarea">Textarea</option>
            <option value="number">Number</option>
            <option value="decimal">Decimal</option>
            <option value="boolean">Boolean</option>
            <option value="date">Date</option>
            <option value="select">Select</option>
          </select>
          {error?.fieldType ? <span className="field__error">{error.fieldType.message}</span> : null}
        </div>
      </div>

      <div className="field-grid field-grid--three-columns">
        <div className="field">
          <label htmlFor={`fieldDefinitions.${index}.displayOrder`}>Display order</label>
          <input
            id={`fieldDefinitions.${index}.displayOrder`}
            type="number"
            min={1}
            {...register(`fieldDefinitions.${index}.displayOrder`)}
          />
          {error?.displayOrder ? (
            <span className="field__error">{error.displayOrder.message}</span>
          ) : null}
        </div>

        <label className="checkbox-field" htmlFor={`fieldDefinitions.${index}.isRequired`}>
          <input
            id={`fieldDefinitions.${index}.isRequired`}
            type="checkbox"
            {...register(`fieldDefinitions.${index}.isRequired`)}
          />
          <span>Required field</span>
        </label>

        <label className="checkbox-field" htmlFor={`fieldDefinitions.${index}.isActive`}>
          <input
            id={`fieldDefinitions.${index}.isActive`}
            type="checkbox"
            {...register(`fieldDefinitions.${index}.isActive`)}
          />
          <span>Field is active</span>
        </label>
      </div>

      <div className="field-grid field-grid--two-columns">
        <div className="field">
          <label htmlFor={`fieldDefinitions.${index}.placeholder`}>Placeholder</label>
          <input
            id={`fieldDefinitions.${index}.placeholder`}
            type="text"
            {...register(`fieldDefinitions.${index}.placeholder`)}
          />
          {error?.placeholder ? (
            <span className="field__error">{error.placeholder.message}</span>
          ) : null}
        </div>

        <div className="field">
          <label htmlFor={`fieldDefinitions.${index}.helpText`}>Help text</label>
          <textarea
            id={`fieldDefinitions.${index}.helpText`}
            rows={2}
            {...register(`fieldDefinitions.${index}.helpText`)}
          />
          {error?.helpText ? <span className="field__error">{error.helpText.message}</span> : null}
        </div>
      </div>

      {fieldType === 'select' ? (
        <section className="select-options-panel">
          <div className="form-section__heading">
            <div>
              <h4>Select options</h4>
              <p>Options are stored as stable values plus display labels.</p>
            </div>
            <button
              className="button--secondary"
              type="button"
              onClick={() => options.append(createDefaultFieldOption(options.fields.length + 1))}
            >
              Add option
            </button>
          </div>

          <div className="entity-field-option-list">
            {options.fields.map((option, optionIndex) => (
              <div className="entity-field-option-row" key={option.id}>
                <div className="field">
                  <label htmlFor={`fieldDefinitions.${index}.options.${optionIndex}.value`}>
                    Option value
                  </label>
                  <input
                    id={`fieldDefinitions.${index}.options.${optionIndex}.value`}
                    type="text"
                    {...register(`fieldDefinitions.${index}.options.${optionIndex}.value`, {
                      onBlur: (event) => {
                        setValue(
                          `fieldDefinitions.${index}.options.${optionIndex}.value`,
                          normalizeEntityTypeCode(event.target.value),
                          { shouldValidate: true },
                        )
                      },
                    })}
                  />
                  {error?.options?.[optionIndex]?.value ? (
                    <span className="field__error">{error.options[optionIndex]?.value?.message}</span>
                  ) : null}
                </div>

                <div className="field">
                  <label htmlFor={`fieldDefinitions.${index}.options.${optionIndex}.label`}>
                    Option label
                  </label>
                  <input
                    id={`fieldDefinitions.${index}.options.${optionIndex}.label`}
                    type="text"
                    {...register(`fieldDefinitions.${index}.options.${optionIndex}.label`)}
                  />
                  {error?.options?.[optionIndex]?.label ? (
                    <span className="field__error">{error.options[optionIndex]?.label?.message}</span>
                  ) : null}
                </div>

                <div className="field">
                  <label htmlFor={`fieldDefinitions.${index}.options.${optionIndex}.displayOrder`}>
                    Display order
                  </label>
                  <input
                    id={`fieldDefinitions.${index}.options.${optionIndex}.displayOrder`}
                    type="number"
                    min={1}
                    {...register(`fieldDefinitions.${index}.options.${optionIndex}.displayOrder`)}
                  />
                  {error?.options?.[optionIndex]?.displayOrder ? (
                    <span className="field__error">
                      {error.options[optionIndex]?.displayOrder?.message}
                    </span>
                  ) : null}
                </div>

                <button
                  className="button--secondary"
                  type="button"
                  onClick={() => options.remove(optionIndex)}
                >
                  Remove option
                </button>
              </div>
            ))}
          </div>

          {'message' in (error?.options ?? {}) && error?.options?.message ? (
            <span className="field__error">{error.options.message}</span>
          ) : null}
        </section>
      ) : null}
    </article>
  )
}
