import { useFieldArray, type Control, type FieldErrors, type UseFormRegister, type UseFormSetValue } from 'react-hook-form'
import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import {
  createDefaultChecklistOption,
  normalizeChecklistOptionValue,
} from '@/modules/preventive-templates/utils/preventive-template-form-utils'

interface PreventiveSelectOptionsEditorProps {
  control: Control<PreventiveTemplateFormValues>
  register: UseFormRegister<PreventiveTemplateFormValues>
  setValue: UseFormSetValue<PreventiveTemplateFormValues>
  sectionIndex: number
  itemIndex: number
  error?: FieldErrors<
    PreventiveTemplateFormValues['sections'][number]['checklistItems'][number]
  >
}

export function PreventiveSelectOptionsEditor({
  control,
  register,
  setValue,
  sectionIndex,
  itemIndex,
  error,
}: PreventiveSelectOptionsEditorProps) {
  const options = useFieldArray({
    control,
    name: `sections.${sectionIndex}.checklistItems.${itemIndex}.options`,
  })

  return (
    <section className="select-options-panel">
      <div className="form-section__heading">
        <div>
          <h4>Select options</h4>
          <p>Options are stored as stable values plus display labels for runtime rendering.</p>
        </div>
        <button
          className="button--secondary"
          type="button"
          onClick={() => options.append(createDefaultChecklistOption(options.fields.length + 1))}
        >
          Add option
        </button>
      </div>

      <div className="entity-field-option-list">
        {options.fields.map((option, optionIndex) => (
          <div className="entity-field-option-row" key={option.id}>
            <div className="field">
              <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.value`}>
                Option value
              </label>
              <input
                id={`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.value`}
                type="text"
                {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.value`, {
                  onBlur: (event) => {
                    setValue(
                      `sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.value`,
                      normalizeChecklistOptionValue(event.target.value),
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
              <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.label`}>
                Option label
              </label>
              <input
                id={`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.label`}
                type="text"
                {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.label`)}
              />
              {error?.options?.[optionIndex]?.label ? (
                <span className="field__error">{error.options[optionIndex]?.label?.message}</span>
              ) : null}
            </div>

            <div className="field">
              <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.displayOrder`}>
                Display order
              </label>
              <input
                id={`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.displayOrder`}
                type="number"
                min={1}
                {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.options.${optionIndex}.displayOrder`)}
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
  )
}
