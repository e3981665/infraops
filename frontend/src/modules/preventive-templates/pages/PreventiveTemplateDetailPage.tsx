import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { preventiveTemplateQueryKeys } from '@/modules/preventive-templates/api/preventive-template-query-keys'
import { preventiveTemplatesClient } from '@/modules/preventive-templates/api/preventive-templates-client'
import {
  buildPreventiveTemplateEditPath,
  routePaths,
} from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'
import {
  localizeChecklistItemType,
  localizeDemoText,
} from '@/shared/i18n/localized-domain-labels'

export function PreventiveTemplateDetailPage() {
  const { locale, t } = useTranslation()
  const { preventiveTemplateId } = useParams()
  const queryClient = useQueryClient()
  const { hasPermission, session } = useAuthSession()
  const accessToken = session?.tokens.accessToken
  const canWrite = hasPermission(permissionCodes.preventiveTemplatesWrite)

  const templateQuery = useQuery({
    queryKey: preventiveTemplateId
      ? preventiveTemplateQueryKeys.detail(preventiveTemplateId)
      : preventiveTemplateQueryKeys.all,
    queryFn: () => preventiveTemplatesClient.getById(preventiveTemplateId!, accessToken!),
    enabled: Boolean(preventiveTemplateId && accessToken),
  })

  const activateMutation = useMutation({
    mutationFn: () => preventiveTemplatesClient.activate(preventiveTemplateId!, accessToken!),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: preventiveTemplateQueryKeys.lists() }),
        queryClient.invalidateQueries({
          queryKey: preventiveTemplateQueryKeys.detail(preventiveTemplateId!),
        }),
      ])
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: () => preventiveTemplatesClient.deactivate(preventiveTemplateId!, accessToken!),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: preventiveTemplateQueryKeys.lists() }),
        queryClient.invalidateQueries({
          queryKey: preventiveTemplateQueryKeys.detail(preventiveTemplateId!),
        }),
      ])
    },
  })

  if (!preventiveTemplateId || !accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.detail.unresolvedTitle')}</h1>
        <p>{t('templates.detail.unresolvedHelp')}</p>
      </section>
    )
  }

  if (templateQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.detail.loadingTitle')}</h1>
        <p>{t('templates.detail.loadingHelp')}</p>
      </section>
    )
  }

  if (templateQuery.isError || !templateQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
        <h1>{t('templates.detail.loadFailed')}</h1>
        <p>{templateQuery.error?.message ?? t('templates.detail.notFound')}</p>
      </section>
    )
  }

  const template = templateQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('templates.detail.administration')}</p>
          <h1>{localizeDemoText(template.name, locale)}</h1>
        </div>
        <p>
          {t('templates.detail.description')}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.preventiveTemplates}>
          {t('common.backToList')}
        </Link>
        {canWrite ? (
          <Link className="button--secondary" to={buildPreventiveTemplateEditPath(template.id)}>
            {t('templates.detail.edit')}
          </Link>
        ) : null}
        {canWrite ? (
          <button
            className="button"
            type="button"
            onClick={() => {
              if (template.isActive) {
                void deactivateMutation.mutateAsync()
                return
              }

              void activateMutation.mutateAsync()
            }}
          >
            {template.isActive ? t('templates.detail.deactivate') : t('templates.detail.activate')}
          </button>
        ) : null}
      </div>

      <div className="detail-grid">
        <article className="card">
          <h2>{t('common.metadata')}</h2>
          <dl className="definition-list">
            <div>
              <dt>{t('common.entityType')}</dt>
              <dd>{localizeDemoText(template.entityTypeName, locale)}</dd>
            </div>
            <div>
              <dt>{t('common.code')}</dt>
              <dd>
                <code>{template.code}</code>
              </dd>
            </div>
            <div>
              <dt>{t('common.status')}</dt>
              <dd>{template.isActive ? t('common.active') : t('common.inactive')}</dd>
            </div>
            <div>
              <dt>{t('templates.sections')}</dt>
              <dd>{template.sections.filter((section) => section.isActive).length}</dd>
            </div>
            <div>
              <dt>{t('entity.form.description')}</dt>
              <dd>{localizeDemoText(template.description, locale) || t('common.noDescription')}</dd>
            </div>
          </dl>
        </article>
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>{t('templates.detail.structureTitle')}</h2>
            <p>{t('templates.detail.structureHelp')}</p>
          </div>
        </div>

        <div className="entity-field-list">
          {template.sections.map((section) => (
            <article
              className={`entity-field-editor${section.isActive ? '' : ' entity-field-editor--inactive'}`}
              key={section.id}
            >
              <div className="entity-field-editor__header">
                <div>
                  <h3>{localizeDemoText(section.title, locale)}</h3>
                  <p>{t('entity.detail.displayOrderWithValue', { value: section.displayOrder })}</p>
                </div>
                <span className={`status-chip${section.isActive ? '' : ' status-chip--inactive'}`}>
                  {section.isActive ? t('common.active') : t('common.inactive')}
                </span>
              </div>

              <div className="entity-field-list">
                {section.checklistItems.map((item) => (
                  <article
                    className={`entity-field-editor${item.isActive ? '' : ' entity-field-editor--inactive'}`}
                    key={item.id}
                  >
                    <div className="entity-field-editor__header">
                      <div>
                        <h4>{localizeDemoText(item.label, locale)}</h4>
                        <p>
                          <code>{item.itemKey}</code>
                        </p>
                      </div>
                      <span className={`status-chip${item.isActive ? '' : ' status-chip--inactive'}`}>
                        {item.isActive ? t('common.active') : t('common.inactive')}
                      </span>
                    </div>

                    <dl className="definition-list definition-list--compact">
                      <div>
                        <dt>{t('templates.item.type')}</dt>
                        <dd>{localizeChecklistItemType(item.itemType, locale)}</dd>
                      </div>
                      <div>
                        <dt>{t('entity.form.displayOrder')}</dt>
                        <dd>{item.displayOrder}</dd>
                      </div>
                      <div>
                        <dt>{t('common.required')}</dt>
                        <dd>{item.isRequired ? t('common.yes') : t('common.no')}</dd>
                      </div>
                      <div>
                        <dt>{t('templates.item.critical')}</dt>
                        <dd>{item.isCritical ? t('common.yes') : t('common.no')}</dd>
                      </div>
                      <div>
                        <dt>{t('templates.detail.commentOnFailure')}</dt>
                        <dd>{item.requiresCommentOnFailure ? t('common.yes') : t('common.no')}</dd>
                      </div>
                      <div>
                        <dt>{t('templates.detail.photoOnFailure')}</dt>
                        <dd>{item.requiresPhotoOnFailure ? t('common.yes') : t('common.no')}</dd>
                      </div>
                      <div>
                        <dt>{t('entity.form.helpText')}</dt>
                        <dd>{localizeDemoText(item.helpText, locale) || t('common.notDefined')}</dd>
                      </div>
                      {item.itemType === 'numeric' ? (
                        <>
                          <div>
                            <dt>{t('templates.item.minimum')}</dt>
                            <dd>{item.minimumValue ?? t('common.notDefined')}</dd>
                          </div>
                          <div>
                            <dt>{t('templates.item.maximum')}</dt>
                            <dd>{item.maximumValue ?? t('common.notDefined')}</dd>
                          </div>
                        </>
                      ) : null}
                    </dl>

                    {item.itemType === 'select' ? (
                      <div className="select-options-panel">
                        <div className="form-section__heading">
                          <div>
                            <h4>{t('templates.options.title')}</h4>
                          </div>
                        </div>
                        <div className="entity-field-option-list">
                          {item.options.map((option) => (
                            <div className="entity-field-option-row" key={option.id}>
                              <div>
                                <strong>{localizeDemoText(option.label, locale)}</strong>
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
            </article>
          ))}
        </div>
      </section>
    </section>
  )
}
