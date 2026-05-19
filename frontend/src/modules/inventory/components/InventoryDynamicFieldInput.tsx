import type { FieldErrors, UseFormRegister } from 'react-hook-form'
import type { InventoryItemFormValues } from '@/modules/inventory/schemas/inventory-form-schema'
import type { InventoryFormFieldDefinition } from '@/modules/inventory/types/inventory'
import { useTranslation } from '@/shared/i18n/useTranslation'
import type { TranslationKey } from '@/shared/i18n/translations'

interface InventoryDynamicFieldInputProps {
  fieldDefinition: InventoryFormFieldDefinition
  register: UseFormRegister<InventoryItemFormValues>
  error?: FieldErrors<InventoryItemFormValues>['attributeValues']
}

export function InventoryDynamicFieldInput({
  fieldDefinition,
  register,
  error,
}: InventoryDynamicFieldInputProps) {
  const { t } = useTranslation()
  const fieldPath = `attributeValues.${fieldDefinition.fieldKey}` as const
  const fieldError = error?.[fieldDefinition.fieldKey]
  const inputId = `inventory-attribute-${fieldDefinition.fieldKey}`

  return (
    <div className="field">
      <label htmlFor={inputId}>
        {fieldDefinition.displayLabel}
        {fieldDefinition.isRequired ? ' *' : ''}
      </label>

      {renderInput(fieldDefinition, inputId, register(fieldPath), t)}

      {fieldDefinition.helpText ? (
        <span className="field__hint">{fieldDefinition.helpText}</span>
      ) : null}

      {fieldError?.message ? (
        <span className="field__error">{fieldError.message as string}</span>
      ) : null}
    </div>
  )
}

function renderInput(
  fieldDefinition: InventoryFormFieldDefinition,
  inputId: string,
  registration: ReturnType<UseFormRegister<InventoryItemFormValues>>,
  t: (key: TranslationKey, values?: Record<string, string | number>) => string,
) {
  switch (fieldDefinition.fieldType) {
    case 'textarea':
      return (
        <textarea
          id={inputId}
          rows={4}
          placeholder={fieldDefinition.placeholder ?? undefined}
          {...registration}
        />
      )
    case 'number':
      return (
        <input
          id={inputId}
          type="number"
          step={1}
          placeholder={fieldDefinition.placeholder ?? undefined}
          {...registration}
        />
      )
    case 'decimal':
      return (
        <input
          id={inputId}
          type="number"
          step="0.01"
          placeholder={fieldDefinition.placeholder ?? undefined}
          {...registration}
        />
      )
    case 'date':
      return <input id={inputId} type="date" {...registration} />
    case 'boolean':
      return (
        <select id={inputId} {...registration}>
          <option value="">{t('inventory.form.selectValue')}</option>
          <option value="true">{t('inventory.form.true')}</option>
          <option value="false">{t('inventory.form.false')}</option>
        </select>
      )
    case 'select':
      return (
        <select id={inputId} {...registration}>
          <option value="">{t('inventory.form.selectOption')}</option>
          {fieldDefinition.options.map((option) => (
            <option key={option.id} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      )
    case 'text':
    default:
      return (
        <input
          id={inputId}
          type="text"
          placeholder={fieldDefinition.placeholder ?? undefined}
          {...registration}
        />
      )
  }
}
