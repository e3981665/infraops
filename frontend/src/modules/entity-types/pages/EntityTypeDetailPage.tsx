import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { entityTypeQueryKeys } from '@/modules/entity-types/api/entity-type-query-keys'
import { entityTypesClient } from '@/modules/entity-types/api/entity-types-client'
import { buildEntityTypeEditPath, routePaths } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function EntityTypeDetailPage() {
  const { t } = useTranslation()
  const { entityTypeId } = useParams()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const entityTypeQuery = useQuery({
    queryKey: entityTypeId ? entityTypeQueryKeys.detail(entityTypeId) : entityTypeQueryKeys.all,
    queryFn: () => entityTypesClient.getById(entityTypeId!, accessToken!),
    enabled: Boolean(entityTypeId && accessToken),
  })

  const activateMutation = useMutation({
    mutationFn: () => entityTypesClient.activate(entityTypeId!, accessToken!),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.all }),
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.detail(entityTypeId!) }),
      ])
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: () => entityTypesClient.deactivate(entityTypeId!, accessToken!),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.all }),
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.detail(entityTypeId!) }),
      ])
    },
  })

  if (!entityTypeId || !accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
        <h1>{t('entity.detail.unresolvedTitle')}</h1>
        <p>{t('entity.detail.unresolvedHelp')}</p>
      </section>
    )
  }

  if (entityTypeQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
        <h1>{t('entity.detail.loadingTitle')}</h1>
        <p>{t('entity.detail.loadingHelp')}</p>
      </section>
    )
  }

  if (entityTypeQuery.isError || !entityTypeQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
        <h1>{t('entity.detail.loadFailed')}</h1>
        <p>{entityTypeQuery.error?.message ?? t('entity.detail.notFound')}</p>
      </section>
    )
  }

  const entityType = entityTypeQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
          <h1>{entityType.name}</h1>
        </div>
        <p>
          {t('entity.detail.description')}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.entityTypes}>
          {t('common.backToList')}
        </Link>
        <Link className="button--secondary" to={buildEntityTypeEditPath(entityType.id)}>
          {t('entity.detail.editDefinition')}
        </Link>
        <button
          className="button"
          type="button"
          onClick={() => {
            if (entityType.isActive) {
              void deactivateMutation.mutateAsync()
              return
            }

            void activateMutation.mutateAsync()
          }}
        >
          {entityType.isActive ? t('entity.detail.deactivate') : t('entity.detail.activate')}
        </button>
      </div>

      <div className="detail-grid">
        <article className="card">
          <h2>{t('common.metadata')}</h2>
          <dl className="definition-list">
            <div>
              <dt>{t('common.code')}</dt>
              <dd>
                <code>{entityType.code}</code>
              </dd>
            </div>
            <div>
              <dt>{t('common.status')}</dt>
              <dd>{entityType.isActive ? t('common.active') : t('common.inactive')}</dd>
            </div>
            <div>
              <dt>{t('entity.detail.fieldCount')}</dt>
              <dd>{entityType.fieldDefinitions.filter((field) => field.isActive).length}</dd>
            </div>
            <div>
              <dt>{t('entity.form.description')}</dt>
              <dd>{entityType.description || t('common.noDescription')}</dd>
            </div>
          </dl>
        </article>
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>{t('entity.detail.dynamicFields')}</h2>
            <p>{t('entity.detail.dynamicFieldsHelp')}</p>
          </div>
        </div>

        <div className="entity-field-list">
          {entityType.fieldDefinitions.map((fieldDefinition) => (
            <article
              className={`entity-field-editor${fieldDefinition.isActive ? '' : ' entity-field-editor--inactive'}`}
              key={fieldDefinition.id}
            >
              <div className="entity-field-editor__header">
                <div>
                  <h3>{fieldDefinition.displayLabel}</h3>
                  <p>
                    <code>{fieldDefinition.fieldKey}</code>
                  </p>
                </div>
                <span className={`status-chip${fieldDefinition.isActive ? '' : ' status-chip--inactive'}`}>
                  {fieldDefinition.isActive ? t('common.active') : t('common.inactive')}
                </span>
              </div>

              <dl className="definition-list definition-list--compact">
                <div>
                  <dt>{t('entity.form.fieldType')}</dt>
                  <dd>{fieldDefinition.fieldType}</dd>
                </div>
                <div>
                  <dt>{t('entity.form.displayOrder')}</dt>
                  <dd>{fieldDefinition.displayOrder}</dd>
                </div>
                <div>
                  <dt>{t('common.required')}</dt>
                  <dd>{fieldDefinition.isRequired ? t('common.yes') : t('common.no')}</dd>
                </div>
                <div>
                  <dt>{t('entity.form.placeholder')}</dt>
                  <dd>{fieldDefinition.placeholder || t('common.notDefined')}</dd>
                </div>
                <div>
                  <dt>{t('entity.form.helpText')}</dt>
                  <dd>{fieldDefinition.helpText || t('common.notDefined')}</dd>
                </div>
              </dl>

              {fieldDefinition.fieldType === 'select' ? (
                <div className="select-options-panel">
                  <div className="form-section__heading">
                    <div>
                      <h4>{t('entity.form.selectOptions')}</h4>
                    </div>
                  </div>
                  <div className="entity-field-option-list">
                    {fieldDefinition.options.map((option) => (
                      <div className="entity-field-option-row" key={option.id}>
                        <div>
                          <strong>{option.label}</strong>
                          <p>
                            <code>{option.value}</code>
                          </p>
                        </div>
                        <span>{t('entity.detail.displayOrderWithValue', { value: option.displayOrder })}</span>
                      </div>
                    ))}
                  </div>
                </div>
              ) : null}
            </article>
          ))}
        </div>
      </section>
    </section>
  )
}
