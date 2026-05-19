import { useEffect, useMemo, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm, useWatch, type UseFormSetError } from 'react-hook-form'
import { ApiError } from '@/shared/api/http-client'
import {
  createInventoryItemFormSchema,
  type InventoryItemFormValues,
} from '@/modules/inventory/schemas/inventory-form-schema'
import { InventoryDynamicFieldInput } from '@/modules/inventory/components/InventoryDynamicFieldInput'
import type {
  InventoryFormDefinition,
  InventoryFormMetadata,
} from '@/modules/inventory/types/inventory'
import {
  alignAttributeValuesWithDefinition,
  areAttributeValueMapsEqual,
  getSitesForRegion,
} from '@/modules/inventory/utils/inventory-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'

interface InventoryItemFormProps {
  eyebrow: string
  title: string
  description: string
  submitLabel: string
  initialValues: InventoryItemFormValues
  metadata: InventoryFormMetadata
  formDefinition: InventoryFormDefinition | null
  isFormDefinitionLoading: boolean
  isEntityTypeLocked?: boolean
  onEntityTypeChange?: (entityTypeId: string) => void
  onSubmit: (values: InventoryItemFormValues) => Promise<void>
}

export function InventoryItemForm({
  eyebrow,
  title,
  description,
  submitLabel,
  initialValues,
  metadata,
  formDefinition,
  isFormDefinitionLoading,
  isEntityTypeLocked = false,
  onEntityTypeChange,
  onSubmit,
}: InventoryItemFormProps) {
  const { t } = useTranslation()
  const [submitError, setSubmitError] = useState<string | null>(null)
  const inventoryItemFormSchema = useMemo(
    () => createInventoryItemFormSchema(formDefinition, t),
    [formDefinition, t],
  )
  const {
    control,
    getValues,
    handleSubmit,
    register,
    reset,
    setError,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<InventoryItemFormValues>({
    resolver: zodResolver(inventoryItemFormSchema),
    defaultValues: initialValues,
  })

  const entityTypeId = useWatch({
    control,
    name: 'entityTypeId',
  })
  const regionId = useWatch({
    control,
    name: 'regionId',
  })
  const siteId = useWatch({
    control,
    name: 'siteId',
  })
  const availableSites = getSitesForRegion(metadata.sites, regionId)

  useEffect(() => {
    reset(initialValues)
  }, [initialValues, reset])

  useEffect(() => {
    onEntityTypeChange?.(entityTypeId)
  }, [entityTypeId, onEntityTypeChange])

  useEffect(() => {
    if (!siteId) {
      return
    }

    if (!availableSites.some((site) => site.id === siteId)) {
      setValue('siteId', '', { shouldValidate: true })
    }
  }, [availableSites, setValue, siteId])

  useEffect(() => {
    const currentAttributeValues = getValues('attributeValues')
    const nextAttributeValues = alignAttributeValuesWithDefinition(
      formDefinition,
      currentAttributeValues,
    )

    if (!areAttributeValueMapsEqual(currentAttributeValues, nextAttributeValues)) {
      setValue('attributeValues', nextAttributeValues, { shouldValidate: false })
    }
  }, [formDefinition, getValues, setValue])

  async function handleFormSubmit(values: InventoryItemFormValues) {
    setSubmitError(null)

    try {
      await onSubmit(values)
    } catch (error) {
      if (error instanceof ApiError) {
        const handled = applyApiValidationErrors(error, setError, formDefinition)
        setSubmitError(handled ? null : error.message)
        return
      }

      setSubmitError(t('inventory.form.saveFailed'))
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
              <h2>{t('inventory.form.metadata')}</h2>
              <p>{t('inventory.form.metadataHelp')}</p>
            </div>
          </div>

          <div className="field-grid field-grid--three-columns">
            <div className="field">
              <label htmlFor="inventoryEntityTypeId">{t('common.entityType')}</label>
              <select
                id="inventoryEntityTypeId"
                disabled={isEntityTypeLocked}
                {...register('entityTypeId')}
              >
                <option value="">{t('inventory.form.selectEntityType')}</option>
                {metadata.entityTypes.map((entityType) => (
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
              <label htmlFor="inventoryRegionId">{t('common.region')}</label>
              <select id="inventoryRegionId" {...register('regionId')}>
                <option value="">{t('inventory.form.selectRegion')}</option>
                {metadata.regions.map((region) => (
                  <option key={region.id} value={region.id}>
                    {region.name}
                  </option>
                ))}
              </select>
              {errors.regionId ? (
                <span className="field__error">{errors.regionId.message}</span>
              ) : null}
            </div>

            <div className="field">
              <label htmlFor="inventorySiteId">{t('common.site')}</label>
              <select id="inventorySiteId" disabled={!regionId} {...register('siteId')}>
                <option value="">{t('inventory.form.selectSite')}</option>
                {availableSites.map((site) => (
                  <option key={site.id} value={site.id}>
                    {site.name}
                  </option>
                ))}
              </select>
              {errors.siteId ? <span className="field__error">{errors.siteId.message}</span> : null}
            </div>
          </div>

          <div className="field-grid field-grid--three-columns">
            <div className="field">
              <label htmlFor="inventoryDisplayName">{t('inventory.displayName')}</label>
              <input id="inventoryDisplayName" type="text" {...register('displayName')} />
              {errors.displayName ? (
                <span className="field__error">{errors.displayName.message}</span>
              ) : null}
            </div>

            <div className="field">
              <label htmlFor="inventoryStatus">{t('common.status')}</label>
              <select id="inventoryStatus" {...register('status')}>
                <option value="">{t('inventory.form.selectStatus')}</option>
                {metadata.statuses.map((status) => (
                  <option key={status.code} value={status.code}>
                    {status.label}
                  </option>
                ))}
              </select>
              {errors.status ? <span className="field__error">{errors.status.message}</span> : null}
            </div>

            <div className="field">
              <label htmlFor="inventoryInstallationDate">{t('inventory.form.installationDate')}</label>
              <input id="inventoryInstallationDate" type="date" {...register('installationDate')} />
              {errors.installationDate ? (
                <span className="field__error">{errors.installationDate.message}</span>
              ) : null}
            </div>
          </div>
        </section>

        <section className="form-section">
          <div className="form-section__heading">
            <div>
              <h2>{t('inventory.form.dynamicAttributes')}</h2>
              <p>{t('inventory.form.dynamicAttributesHelp')}</p>
            </div>
          </div>

          {!metadata.entityTypes.length ? (
            <div className="empty-state">
              <h2>{t('inventory.form.noActiveEntityTypes')}</h2>
              <p>{t('inventory.form.noActiveEntityTypesHelp')}</p>
            </div>
          ) : null}

          {metadata.entityTypes.length > 0 && !entityTypeId ? (
            <div className="empty-state">
              <h2>{t('inventory.form.selectEntityTypeTitle')}</h2>
              <p>{t('inventory.form.selectEntityTypeHelp')}</p>
            </div>
          ) : null}

          {metadata.entityTypes.length > 0 && entityTypeId && isFormDefinitionLoading ? (
            <div className="empty-state">
              <h2>{t('inventory.form.loadingDefinition')}</h2>
              <p>{t('inventory.form.loadingDefinitionHelp')}</p>
            </div>
          ) : null}

          {metadata.entityTypes.length > 0 &&
          entityTypeId &&
          !isFormDefinitionLoading &&
          formDefinition ? (
            <div className="dynamic-field-list">
              {formDefinition.fieldDefinitions.map((fieldDefinition) => (
                <div className="dynamic-field-card" key={fieldDefinition.id}>
                  <InventoryDynamicFieldInput
                    fieldDefinition={fieldDefinition}
                    register={register}
                    error={errors.attributeValues}
                  />
                </div>
              ))}
            </div>
          ) : null}

          {metadata.entityTypes.length > 0 &&
          entityTypeId &&
          !isFormDefinitionLoading &&
          !formDefinition ? (
            <div className="empty-state">
              <h2>{t('inventory.form.definitionLoadFailed')}</h2>
              <p>{t('inventory.form.definitionLoadFailedHelp')}</p>
            </div>
          ) : null}
        </section>

        {submitError ? <p className="form-error">{submitError}</p> : null}

        <div className="form-actions">
          <button className="button" type="submit" disabled={isSubmitting || isFormDefinitionLoading}>
            {isSubmitting ? t('common.saving') : submitLabel}
          </button>
        </div>
      </form>
    </section>
  )
}

function applyApiValidationErrors(
  error: ApiError,
  setError: UseFormSetError<InventoryItemFormValues>,
  formDefinition: InventoryFormDefinition | null,
) {
  if (!error.errors) {
    return false
  }

  let handled = false

  Object.entries(error.errors).forEach(([propertyName, messages]) => {
    const message = messages[0] ?? error.message

    if (propertyName === 'EntityTypeId') {
      setError('entityTypeId', { type: 'server', message })
      handled = true
      return
    }

    if (propertyName === 'RegionId') {
      setError('regionId', { type: 'server', message })
      handled = true
      return
    }

    if (propertyName === 'SiteId') {
      setError('siteId', { type: 'server', message })
      handled = true
      return
    }

    if (propertyName === 'DisplayName') {
      setError('displayName', { type: 'server', message })
      handled = true
      return
    }

    if (propertyName === 'Status') {
      setError('status', { type: 'server', message })
      handled = true
      return
    }

    if (propertyName === 'InstallationDate') {
      setError('installationDate', { type: 'server', message })
      handled = true
      return
    }

    const attributeValueMatch = /^AttributeValues\[(\d+)\]\.(FieldKey|Value)$/.exec(propertyName)

    if (!attributeValueMatch || !formDefinition) {
      return
    }

    const fieldDefinition = [...formDefinition.fieldDefinitions].sort(
      (left, right) => left.displayOrder - right.displayOrder,
    )[Number(attributeValueMatch[1])]

    if (!fieldDefinition) {
      return
    }

    setError(`attributeValues.${fieldDefinition.fieldKey}`, {
      type: 'server',
      message,
    })
    handled = true
  })

  return handled
}
