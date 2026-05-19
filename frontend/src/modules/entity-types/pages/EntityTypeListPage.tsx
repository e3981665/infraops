import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { entityTypeQueryKeys } from '@/modules/entity-types/api/entity-type-query-keys'
import { entityTypesClient } from '@/modules/entity-types/api/entity-types-client'
import {
  buildEntityTypeDetailPath,
  buildEntityTypeEditPath,
  routePaths,
} from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function EntityTypeListPage() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken

  const entityTypesQuery = useQuery({
    queryKey: entityTypeQueryKeys.all,
    queryFn: () => entityTypesClient.list(accessToken!),
    enabled: Boolean(accessToken),
  })

  const activateMutation = useMutation({
    mutationFn: (entityTypeId: string) => entityTypesClient.activate(entityTypeId, accessToken!),
    onSuccess: async (_, entityTypeId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.all }),
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.detail(entityTypeId) }),
      ])
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: (entityTypeId: string) => entityTypesClient.deactivate(entityTypeId, accessToken!),
    onSuccess: async (_, entityTypeId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.all }),
        queryClient.invalidateQueries({ queryKey: entityTypeQueryKeys.detail(entityTypeId) }),
      ])
    },
  })

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.list.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
        <p>{t('templates.authMessage')}</p>
      </section>
    )
  }

  if (entityTypesQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.list.eyebrow')}</p>
        <h1>{t('entity.list.loadingTitle')}</h1>
        <p>{t('entity.list.loadingHelp')}</p>
      </section>
    )
  }

  if (entityTypesQuery.isError) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('entity.list.eyebrow')}</p>
        <h1>{t('entity.list.errorTitle')}</h1>
        <p>{entityTypesQuery.error.message}</p>
      </section>
    )
  }

  const entityTypes = entityTypesQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('entity.detail.administration')}</p>
          <h1>{t('entity.list.title')}</h1>
        </div>
        <p>
          {t('entity.list.description')}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button" to={routePaths.entityTypeCreate}>
          {t('entity.list.create')}
        </Link>
      </div>

      {entityTypes.length === 0 ? (
        <div className="empty-state">
          <h2>{t('entity.list.emptyTitle')}</h2>
          <p>{t('entity.list.emptyMessage')}</p>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">{t('common.name')}</th>
                <th scope="col">{t('common.code')}</th>
                <th scope="col">{t('common.status')}</th>
                <th scope="col">{t('common.fields')}</th>
                <th scope="col">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {entityTypes.map((entityType) => (
                <tr key={entityType.id}>
                  <td>
                    <strong>{entityType.name}</strong>
                    {entityType.description ? <small>{entityType.description}</small> : null}
                  </td>
                  <td>
                    <code>{entityType.code}</code>
                  </td>
                  <td>
                    <span className={`status-chip${entityType.isActive ? '' : ' status-chip--inactive'}`}>
                      {entityType.isActive ? t('common.active') : t('common.inactive')}
                    </span>
                  </td>
                  <td>{entityType.fieldCount}</td>
                  <td>
                    <div className="inline-actions">
                      <Link className="button--secondary" to={buildEntityTypeDetailPath(entityType.id)}>
                        {t('common.view')}
                      </Link>
                      <Link className="button--secondary" to={buildEntityTypeEditPath(entityType.id)}>
                        {t('common.edit')}
                      </Link>
                      <button
                        className="button--secondary"
                        type="button"
                        onClick={() => {
                          if (entityType.isActive) {
                            void deactivateMutation.mutateAsync(entityType.id)
                            return
                          }

                          void activateMutation.mutateAsync(entityType.id)
                        }}
                      >
                        {entityType.isActive ? t('common.deactivate') : t('common.activate')}
                      </button>
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
