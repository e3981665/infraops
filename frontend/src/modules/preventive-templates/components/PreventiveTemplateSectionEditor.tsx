import { useFieldArray, useWatch, type Control, type FieldErrors, type UseFormRegister, type UseFormSetValue } from 'react-hook-form'
import type { PreventiveTemplateFormValues } from '@/modules/preventive-templates/schemas/preventive-template-form-schema'
import { PreventiveChecklistItemEditor } from '@/modules/preventive-templates/components/PreventiveChecklistItemEditor'
import { createDefaultChecklistItem } from '@/modules/preventive-templates/utils/preventive-template-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

interface PreventiveTemplateSectionEditorProps {
  control: Control<PreventiveTemplateFormValues>
  register: UseFormRegister<PreventiveTemplateFormValues>
  setValue: UseFormSetValue<PreventiveTemplateFormValues>
  sectionIndex: number
  onRemoveSection: () => void
  error?: FieldErrors<PreventiveTemplateFormValues['sections'][number]>
  isPersisted: boolean
}

export function PreventiveTemplateSectionEditor({
  control,
  register,
  setValue,
  sectionIndex,
  onRemoveSection,
  error,
  isPersisted,
}: PreventiveTemplateSectionEditorProps) {
  const { t } = useTranslation()
  const isActive = useWatch({
    control,
    name: `sections.${sectionIndex}.isActive`,
  })
  const checklistItems = useFieldArray({
    control,
    name: `sections.${sectionIndex}.checklistItems`,
  })

  return (
    <article className={`entity-field-editor${isActive ? '' : ' entity-field-editor--inactive'}`}>
      <div className="entity-field-editor__header">
        <div>
          <h3>{t('templates.section.title', { index: sectionIndex + 1 })}</h3>
          <p>{isActive ? t('templates.section.active') : t('templates.section.inactive')}</p>
        </div>
        <div className="button-row">
          {isPersisted ? (
            <button
              className="button--secondary"
              type="button"
              onClick={() => {
                setValue(`sections.${sectionIndex}.isActive`, !isActive, {
                  shouldValidate: true,
                })
              }}
            >
              {isActive ? t('templates.section.deactivate') : t('templates.section.reactivate')}
            </button>
          ) : (
            <button className="button--secondary" type="button" onClick={onRemoveSection}>
              {t('templates.section.remove')}
            </button>
          )}
        </div>
      </div>

      <div className="field-grid field-grid--three-columns">
        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.title`}>{t('templates.section.titleLabel')}</label>
          <input
            id={`sections.${sectionIndex}.title`}
            type="text"
            {...register(`sections.${sectionIndex}.title`)}
          />
          {error?.title ? <span className="field__error">{error.title.message}</span> : null}
        </div>

        <div className="field">
          <label htmlFor={`sections.${sectionIndex}.displayOrder`}>{t('entity.form.displayOrder')}</label>
          <input
            id={`sections.${sectionIndex}.displayOrder`}
            type="number"
            min={1}
            {...register(`sections.${sectionIndex}.displayOrder`)}
          />
          {error?.displayOrder ? (
            <span className="field__error">{error.displayOrder.message}</span>
          ) : null}
        </div>

        <label className="checkbox-field" htmlFor={`sections.${sectionIndex}.isActive`}>
          <input
            id={`sections.${sectionIndex}.isActive`}
            type="checkbox"
            {...register(`sections.${sectionIndex}.isActive`)}
          />
          <span>{t('templates.section.isActive')}</span>
        </label>
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h4>{t('templates.checklistItems')}</h4>
            <p>{t('templates.section.itemsHelp')}</p>
          </div>
          <button
            className="button--secondary"
            type="button"
            onClick={() => checklistItems.append(createDefaultChecklistItem(checklistItems.fields.length + 1))}
          >
            {t('templates.section.addItem')}
          </button>
        </div>

        <div className="entity-field-list">
          {checklistItems.fields.map((item, itemIndex) => (
            <PreventiveChecklistItemEditor
              key={item.id}
              control={control}
              register={register}
              setValue={setValue}
              sectionIndex={sectionIndex}
              itemIndex={itemIndex}
              onRemoveItem={() => checklistItems.remove(itemIndex)}
              error={
                error?.checklistItems?.[itemIndex] as
                  | FieldErrors<PreventiveTemplateFormValues['sections'][number]['checklistItems'][number]>
                  | undefined
              }
              isPersisted={Boolean(item.id)}
            />
          ))}
        </div>

        {'message' in (error?.checklistItems ?? {}) && error?.checklistItems?.message ? (
          <span className="field__error">{error.checklistItems.message}</span>
        ) : null}
      </section>
    </article>
  )
}
