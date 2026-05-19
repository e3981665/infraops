import { useEffect } from 'react'
import { useWatch, type Control, type FieldErrors, type UseFormRegister, type UseFormSetValue } from 'react-hook-form'
import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import { PreventiveSelectOptionsEditor } from '@/modules/preventive-templates/components/PreventiveSelectOptionsEditor'
import { normalizeChecklistItemKey } from '@/modules/preventive-templates/utils/preventive-template-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

interface PreventiveChecklistItemEditorProps {
  control: Control<PreventiveTemplateFormValues>
  register: UseFormRegister<PreventiveTemplateFormValues>
  setValue: UseFormSetValue<PreventiveTemplateFormValues>
  sectionIndex: number
  itemIndex: number
  onRemoveItem: () => void
  error?: FieldErrors<
    PreventiveTemplateFormValues['sections'][number]['checklistItems'][number]
  >
  isPersisted: boolean
}

export function PreventiveChecklistItemEditor({
  control,
  register,
  setValue,
  sectionIndex,
  itemIndex,
  onRemoveItem,
  error,
  isPersisted,
}: PreventiveChecklistItemEditorProps) {
  const { t } = useTranslation()
  const itemType = useWatch({
    control,
    name: `sections.${sectionIndex}.checklistItems.${itemIndex}.itemType`,
  })
  const isActive = useWatch({
    control,
    name: `sections.${sectionIndex}.checklistItems.${itemIndex}.isActive`,
  })
  const showsFailureRequirements = itemType !== 'text'

  useEffect(() => {
    if (itemType !== 'select') {
      setValue(`sections.${sectionIndex}.checklistItems.${itemIndex}.options`, [], {
        shouldValidate: false,
      })
    }
  }, [itemType, sectionIndex, itemIndex, setValue])

  useEffect(() => {
    if (itemType !== 'numeric') {
      setValue(`sections.${sectionIndex}.checklistItems.${itemIndex}.minimumValue`, '', {
        shouldValidate: false,
      })
      setValue(`sections.${sectionIndex}.checklistItems.${itemIndex}.maximumValue`, '', {
        shouldValidate: false,
      })
    }
  }, [itemIndex, itemType, sectionIndex, setValue])

  return (
    <article className={`entity-field-editor${isActive ? '' : ' entity-field-editor--inactive'}`}>
      <div className="entity-field-editor__header">
        <div>
          <h4>{t('templates.item.title', { index: itemIndex + 1 })}</h4>
          <p>{isActive ? t('templates.item.active') : t('templates.item.inactive')}</p>
        </div>
        <div className="button-row">
          {isPersisted ? (
            <button
              className="button--secondary"
              type="button"
              onClick={() => {
                setValue(`sections.${sectionIndex}.checklistItems.${itemIndex}.isActive`, !isActive, {
                  shouldValidate: true,
                })
              }}
            >
              {isActive ? t('templates.item.deactivate') : t('templates.item.reactivate')}
            </button>
          ) : (
            <button className="button--secondary" type="button" onClick={onRemoveItem}>
              {t('templates.item.remove')}
            </button>
          )}
        </div>
      </div>

      <div className="field-grid field-grid--three-columns">
        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.itemKey`}>{t('templates.item.key')}</label>
          <input
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.itemKey`}
            type="text"
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.itemKey`, {
              onBlur: (event) => {
                setValue(
                  `sections.${sectionIndex}.checklistItems.${itemIndex}.itemKey`,
                  normalizeChecklistItemKey(event.target.value),
                  { shouldValidate: true },
                )
              },
            })}
          />
          {error?.itemKey ? <span className="field__error">{error.itemKey.message}</span> : null}
        </div>

        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.label`}>{t('templates.item.label')}</label>
          <input
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.label`}
            type="text"
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.label`)}
          />
          {error?.label ? <span className="field__error">{error.label.message}</span> : null}
        </div>

        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.itemType`}>{t('templates.item.type')}</label>
          <select
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.itemType`}
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.itemType`)}
          >
            <option value="yesNo">{t('templates.item.yesNo')}</option>
            <option value="text">{t('templates.item.text')}</option>
            <option value="numeric">{t('templates.item.numeric')}</option>
            <option value="select">{t('templates.item.select')}</option>
          </select>
          {error?.itemType ? <span className="field__error">{error.itemType.message}</span> : null}
        </div>
      </div>

      <div className="field-grid field-grid--three-columns">
        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.displayOrder`}>{t('entity.form.displayOrder')}</label>
          <input
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.displayOrder`}
            type="number"
            min={1}
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.displayOrder`)}
          />
          {error?.displayOrder ? <span className="field__error">{error.displayOrder.message}</span> : null}
        </div>

        <label className="checkbox-field" htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.isRequired`}>
          <input
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.isRequired`}
            type="checkbox"
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.isRequired`)}
          />
          <span>{t('templates.item.required')}</span>
        </label>

        <label className="checkbox-field" htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.isActive`}>
          <input
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.isActive`}
            type="checkbox"
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.isActive`)}
          />
          <span>{t('templates.item.isActive')}</span>
        </label>
      </div>

      <div className="field-grid field-grid--two-columns">
        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.helpText`}>{t('entity.form.helpText')}</label>
          <textarea
            id={`sections.${sectionIndex}.checklistItems.${itemIndex}.helpText`}
            rows={2}
            {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.helpText`)}
          />
          {error?.helpText ? <span className="field__error">{error.helpText.message}</span> : null}
        </div>

        <div className="field-grid field-grid--two-columns">
          <label className="checkbox-field" htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.isCritical`}>
            <input
              id={`sections.${sectionIndex}.checklistItems.${itemIndex}.isCritical`}
              type="checkbox"
              {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.isCritical`)}
            />
            <span>{t('templates.item.critical')}</span>
          </label>

          {showsFailureRequirements ? (
            <>
              <label
                className="checkbox-field"
                htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.requiresCommentOnFailure`}
              >
                <input
                  id={`sections.${sectionIndex}.checklistItems.${itemIndex}.requiresCommentOnFailure`}
                  type="checkbox"
                  {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.requiresCommentOnFailure`)}
                />
                <span>{t('templates.item.commentOnFailure')}</span>
              </label>

              <label
                className="checkbox-field"
                htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.requiresPhotoOnFailure`}
              >
                <input
                  id={`sections.${sectionIndex}.checklistItems.${itemIndex}.requiresPhotoOnFailure`}
                  type="checkbox"
                  {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.requiresPhotoOnFailure`)}
                />
                <span>{t('templates.item.photoOnFailure')}</span>
              </label>
            </>
          ) : null}
        </div>
      </div>

      {itemType === 'numeric' ? (
        <div className="field-grid field-grid--two-columns">
          <div className="field">
            <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.minimumValue`}>{t('templates.item.minimum')}</label>
            <input
              id={`sections.${sectionIndex}.checklistItems.${itemIndex}.minimumValue`}
              type="number"
              step="0.01"
              {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.minimumValue`)}
            />
            {error?.minimumValue ? (
              <span className="field__error">{error.minimumValue.message}</span>
            ) : null}
          </div>

          <div className="field">
            <label htmlFor={`sections.${sectionIndex}.checklistItems.${itemIndex}.maximumValue`}>{t('templates.item.maximum')}</label>
            <input
              id={`sections.${sectionIndex}.checklistItems.${itemIndex}.maximumValue`}
              type="number"
              step="0.01"
              {...register(`sections.${sectionIndex}.checklistItems.${itemIndex}.maximumValue`)}
            />
            {error?.maximumValue ? (
              <span className="field__error">{error.maximumValue.message}</span>
            ) : null}
          </div>
        </div>
      ) : null}

      {itemType === 'select' ? (
        <PreventiveSelectOptionsEditor
          control={control}
          register={register}
          setValue={setValue}
          sectionIndex={sectionIndex}
          itemIndex={itemIndex}
          error={error}
        />
      ) : null}
    </article>
  )
}
