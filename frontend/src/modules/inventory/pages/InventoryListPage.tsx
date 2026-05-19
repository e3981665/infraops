import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { StatusBadge } from '@/components/status/StatusBadge'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import type { InventoryListFilters } from '@/modules/inventory/types/inventory'
import { getSitesForRegion } from '@/modules/inventory/utils/inventory-form-utils'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'
import {
  buildInventoryDetailPath,
  buildInventoryEditPath,
  routePaths,
} from '@/shared/routing/route-paths'

const defaultFilters: InventoryListFilters = {
  entityTypeId: '',
  status: '',
  siteId: '',
  regionId: '',
  isActive: '',
  search: '',
}

export function InventoryListPage() {
  const queryClient = useQueryClient()
  const { hasPermission, session } = useAuthSession()
  const { locale, t } = useTranslation()
  const accessToken = session?.tokens.accessToken
  const [filters, setFilters] = useState<InventoryListFilters>(defaultFilters)
  const canWrite = hasPermission(permissionCodes.inventoryWrite)

  const metadataQuery = useQuery({
    queryKey: inventoryQueryKeys.formMetadata,
    queryFn: () => inventoryClient.getFormMetadata(accessToken!),
    enabled: Boolean(accessToken),
  })

  const inventoryItemsQuery = useQuery({
    queryKey: inventoryQueryKeys.list(filters),
    queryFn: () => inventoryClient.list(filters, accessToken!),
    enabled: Boolean(accessToken),
  })

  const activateMutation = useMutation({
    mutationFn: (inventoryItemId: string) => inventoryClient.activate(inventoryItemId, accessToken!),
    onSuccess: async (_, inventoryItemId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.lists() }),
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.detail(inventoryItemId) }),
      ])
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: (inventoryItemId: string) =>
      inventoryClient.deactivate(inventoryItemId, accessToken!),
    onSuccess: async (_, inventoryItemId) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.lists() }),
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.detail(inventoryItemId) }),
      ])
    },
  })

  const availableSites = getSitesForRegion(metadataQuery.data?.sites ?? [], filters.regionId ?? '')
  const selectedSiteId = availableSites.some((site) => site.id === filters.siteId)
    ? filters.siteId
    : ''

  if (!accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('common.authRequired')}</h1>
        <p>{t('inventory.authMessage')}</p>
      </section>
    )
  }

  if (metadataQuery.isLoading || inventoryItemsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.loadingTitle')}</h1>
        <p>{t('inventory.loadingMessage')}</p>
      </section>
    )
  }

  if (metadataQuery.isError || inventoryItemsQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.errorTitle')}</h1>
        <p>{metadataQuery.error?.message ?? inventoryItemsQuery.error?.message}</p>
      </section>
    )
  }

  const inventoryItems = inventoryItemsQuery.data ?? []
  const metadata = metadataQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('inventory.management')}</p>
          <h1>{t('inventory.listTitle')}</h1>
        </div>
        <p>{t('inventory.listDescription')}</p>
      </div>

      <div className="module-panel__actions">
        {canWrite ? (
          <Link className="button" to={routePaths.inventoryCreate}>
            {t('inventory.register')}
          </Link>
        ) : null}
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>{t('common.filters')}</h2>
            <p>{t('inventory.filterHelp')}</p>
          </div>
        </div>

        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="inventoryFilterEntityType">{t('common.entityType')}</label>
            <select
              id="inventoryFilterEntityType"
              value={filters.entityTypeId}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  entityTypeId: event.target.value,
                }))
              }}
            >
              <option value="">{t('common.allEntityTypes')}</option>
              {metadata.entityTypes.map((entityType) => (
                <option key={entityType.id} value={entityType.id}>
                  {localizeDemoText(entityType.name, locale)}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterRegion">{t('common.region')}</label>
            <select
              id="inventoryFilterRegion"
              value={filters.regionId}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  regionId: event.target.value,
                  siteId: '',
                }))
              }}
            >
              <option value="">{t('common.allRegions')}</option>
              {metadata.regions.map((region) => (
                <option key={region.id} value={region.id}>
                  {localizeDemoText(region.name, locale)}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterSite">{t('common.site')}</label>
            <select
              id="inventoryFilterSite"
              value={selectedSiteId}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  siteId: event.target.value,
                }))
              }}
            >
              <option value="">{t('common.allSites')}</option>
              {availableSites.map((site) => (
                <option key={site.id} value={site.id}>
                  {localizeDemoText(site.name, locale)}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="inventoryFilterStatus">{t('common.status')}</label>
            <select
              id="inventoryFilterStatus"
              value={filters.status}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  status: event.target.value,
                }))
              }}
            >
              <option value="">{t('common.allStatuses')}</option>
              {metadata.statuses.map((status) => (
                <option key={status.code} value={status.code}>
                  {localizeDemoText(status.label, locale)}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterActivity">{t('common.lifecycle')}</label>
            <select
              id="inventoryFilterActivity"
              value={filters.isActive}
              onChange={(event) => {
                setFilters((currentFilters) => ({
                  ...currentFilters,
                  isActive: event.target.value,
                }))
              }}
            >
              <option value="">{t('inventory.allItems')}</option>
              <option value="true">{t('inventory.activeOnly')}</option>
              <option value="false">{t('inventory.inactiveOnly')}</option>
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterSearch">{t('common.search')}</label>
            <input
              id="inventoryFilterSearch"
              type="text"
              value={filters.search}
              placeholder={t('inventory.searchPlaceholder')}
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

      {inventoryItems.length === 0 ? (
        <div className="empty-state">
          <h2>{t('inventory.emptyTitle')}</h2>
          <p>{t('inventory.emptyMessage')}</p>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">{t('inventory.displayName')}</th>
                <th scope="col">{t('common.entityType')}</th>
                <th scope="col">{t('common.region')}</th>
                <th scope="col">{t('common.site')}</th>
                <th scope="col">{t('common.status')}</th>
                <th scope="col">{t('common.lifecycle')}</th>
                <th scope="col">{t('common.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {inventoryItems.map((inventoryItem) => (
                <tr key={inventoryItem.id}>
                  <td>
                    <strong>{inventoryItem.displayName}</strong>
                    {inventoryItem.installationDate ? (
                      <small>{t('inventory.installed', { date: inventoryItem.installationDate })}</small>
                    ) : null}
                  </td>
                  <td>{localizeDemoText(inventoryItem.entityTypeName, locale)}</td>
                  <td>{localizeDemoText(inventoryItem.regionName, locale)}</td>
                  <td>{localizeDemoText(inventoryItem.siteName, locale)}</td>
                  <td>
                    <StatusBadge value={inventoryItem.status} />
                  </td>
                  <td>
                    <span
                      className={`status-chip${
                        inventoryItem.isActive ? '' : ' status-chip--inactive'
                      }`}
                    >
                      {inventoryItem.isActive ? t('common.active') : t('common.inactive')}
                    </span>
                  </td>
                  <td>
                    <div className="inline-actions">
                      <Link className="button--secondary" to={buildInventoryDetailPath(inventoryItem.id)}>
                        {t('common.view')}
                      </Link>
                      {canWrite ? (
                        <Link className="button--secondary" to={buildInventoryEditPath(inventoryItem.id)}>
                          {t('common.edit')}
                        </Link>
                      ) : null}
                      {canWrite ? (
                        <button
                          className="button--secondary"
                          type="button"
                          onClick={() => {
                            if (inventoryItem.isActive) {
                              void deactivateMutation.mutateAsync(inventoryItem.id)
                              return
                            }

                            void activateMutation.mutateAsync(inventoryItem.id)
                          }}
                        >
                          {inventoryItem.isActive ? t('common.deactivate') : t('common.activate')}
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
