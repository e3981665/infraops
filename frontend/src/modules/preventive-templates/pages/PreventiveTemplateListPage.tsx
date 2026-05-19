import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { preventiveTemplateQueryKeys } from '@/modules/preventive-templates/api/preventive-template-query-keys'
import { preventiveTemplatesClient } from '@/modules/preventive-templates/api/preventive-templates-client'
import type { PreventiveTemplateListFilters } from '@/modules/preventive-templates/types/preventive-template'
import { useTranslation } from '@/shared/i18n/useTranslation'
import {
  buildPreventiveTemplateDetailPath,
  buildPreventiveTemplateEditPath,
  routePaths,
} from '@/shared/routing/route-paths'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

const defaultFilters: PreventiveTemplateListFilters = {
  entityTypeId: '',
  isActive: '',
  search: '',
}

export function PreventiveTemplateListPage() {
  const queryClient = useQueryClient()
  const { hasPermission, session } = useAuthSession()
  const { locale, t } = useTranslation()
  const accessToken = session?.tokens.accessToken
  const canWrite = hasPermission(permissionCodes.preventiveTemplatesWrite)
  const [filters, setFilters] = useState<PreventiveTemplateListFilters>(defaultFilters)

  const metadataQuery = useQuery({
    queryKey: preventiveTemplateQueryKeys.formMetadata,
    queryFn: () => preventiveTemplatesClient.getFormMetadata(accessToken!),
    enabled: Boolean(accessToken),
  })

  const templatesQuery = useQuery({
    queryKey: preventiveTemplateQueryKeys.list(filters),
    queryFn: () => preventiveTemplatesClient.list(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  const activateMutation = useMutation({
    mutationFn: (preventiveTemplateId: string) =>
      preventiveTemplatesClient.activate(preventiveTemplateId, accessToken!),
    onSuccess: async (_, preventiveTemplateId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: preventiveTemplateQueryKeys.lists() }),
        queryClient.invalidateQueries({
          queryKey: preventiveTemplateQueryKeys.detail(preventiveTemplateId),
        }),
      ])
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: (preventiveTemplateId: string) =>
      preventiveTemplatesClient.deactivate(preventiveTemplateId, accessToken!),
    onSuccess: async (_, preventiveTemplateId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: preventiveTemplateQueryKeys.lists() }),
        queryClient.invalidateQueries({
          queryKey: preventiveTemplateQueryKeys.detail(preventiveTemplateId),
        }),
      ])
    },
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
        <p>{t('templates.authMessage')}</p>
      </section>
    )
  }

  if (metadataQuery.isLoading || templatesQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.eyebrow')}</p>
        <h1>{t('templates.loadingTitle')}</h1>
        <p>{t('templates.loadingMessage')}</p>
      </section>
    )
  }

  if (metadataQuery.isError || templatesQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('templates.eyebrow')}</p>
        <h1>{t('templates.errorTitle')}</h1>
        <p>{metadataQuery.error?.message ?? templatesQuery.error?.message}</p>
      </section>
    )
  }

  const templates = templatesQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('templates.management')}</p>
          <h1>{t('templates.listTitle')}</h1>
        </div>
        <p>{t('templates.listDescription')}</p>
      </div>

      <div className="module-panel__actions">
        {canWrite ? (
          <Link className="button" to={routePaths.preventiveTemplateCreate}>
            {t('templates.create')}
          </Link>
        ) : null}
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>{t('common.filters')}</h2>
            <p>{t('templates.filterHelp')}</p>
          </div>
        </div>

        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="preventiveTemplateFilterEntityType">{t('common.entityType')}</label>
            <select
              id="preventiveTemplateFilterEntityType"
              value={filters.entityTypeId}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  entityTypeId: event.target.value,
                }))
              }}
            >
              <option value="">{t('common.allEntityTypes')}</option>
              {metadataQuery.data.entityTypes.map((entityType) => (
                <option key={entityType.id} value={entityType.id}>
                  {localizeDemoText(entityType.name, locale)}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="preventiveTemplateFilterActivity">{t('common.lifecycle')}</label>
            <select
              id="preventiveTemplateFilterActivity"
              value={filters.isActive}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  isActive: event.target.value,
                }))
              }}
            >
              <option value="">{t('templates.allTemplates')}</option>
              <option value="true">{t('inventory.activeOnly')}</option>
              <option value="false">{t('inventory.inactiveOnly')}</option>
            </select>
          </div>

          <div className="field">
            <label htmlFor="preventiveTemplateFilterSearch">{t('common.search')}</label>
            <input
              id="preventiveTemplateFilterSearch"
              type="text"
              value={filters.search}
              placeholder={t('templates.searchPlaceholder')}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  search: event.target.value,
                }))
              }}
            />
          </div>
        </div>
      </section>

      {templates.length === 0 ? (
        <div className="empty-state">
          <h2>{t('templates.emptyTitle')}</h2>
          <p>{t('templates.emptyMessage')}</p>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">{t('templates.name')}</th>
                <th scope="col">{t('templates.code')}</th>
                <th scope="col">{t('common.entityType')}</th>
                <th scope="col">{t('common.status')}</th>
                <th scope="col">{t('templates.sections')}</th>
                <th scope="col">{t('templates.checklistItems')}</th>
                <th scope="col">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {templates.map((template) => (
                <tr key={template.id}>
                  <td>
                    <strong>{localizeDemoText(template.name, locale)}</strong>
                    {template.description ? (
                      <small>{localizeDemoText(template.description, locale)}</small>
                    ) : null}
                  </td>
                  <td>
                    <code>{template.code}</code>
                  </td>
                  <td>{localizeDemoText(template.entityTypeName, locale)}</td>
                  <td>
                    <span className={`status-chip${template.isActive ? '' : ' status-chip--inactive'}`}>
                      {template.isActive ? t('common.active') : t('common.inactive')}
                    </span>
                  </td>
                  <td>{template.sectionCount}</td>
                  <td>{template.checklistItemCount}</td>
                  <td>
                    <div className="inline-actions">
                      <Link
                        className="button--secondary"
                        to={buildPreventiveTemplateDetailPath(template.id)}
                      >
                        {t('common.view')}
                      </Link>
                      {canWrite ? (
                        <Link
                          className="button--secondary"
                          to={buildPreventiveTemplateEditPath(template.id)}
                        >
                          {t('common.edit')}
                        </Link>
                      ) : null}
                      {canWrite ? (
                        <button
                          className="button--secondary"
                          type="button"
                          onClick={() => {
                            if (template.isActive) {
                              void deactivateMutation.mutateAsync(template.id)
                              return
                            }

                            void activateMutation.mutateAsync(template.id)
                          }}
                        >
                          {template.isActive ? t('common.deactivate') : t('common.activate')}
                        </button>
                      ) : null}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
