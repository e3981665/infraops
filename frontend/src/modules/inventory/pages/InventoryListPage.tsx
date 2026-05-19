import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import type { InventoryListFilters } from '@/modules/inventory/types/inventory'
import { getSitesForRegion } from '@/modules/inventory/utils/inventory-form-utils'
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
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Authenticated access is required.</h1>
        <p>The inventory workspace is waiting for a valid session.</p>
      </section>
    )
  }

  if (metadataQuery.isLoading || inventoryItemsQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Loading registered infrastructure.</h1>
        <p>InfraOps is fetching the current inventory catalog and filter metadata.</p>
      </section>
    )
  }

  if (metadataQuery.isError || inventoryItemsQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Inventory could not be loaded.</h1>
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
          <p className="hero-panel__eyebrow">Inventory management</p>
          <h1>Registered infrastructure inventory.</h1>
        </div>
        <p>
          Register, review, and maintain infrastructure items against the active
          entity type definitions.
        </p>
      </div>

      <div className="module-panel__actions">
        {canWrite ? (
          <Link className="button" to={routePaths.inventoryCreate}>
            Register inventory item
          </Link>
        ) : null}
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>Filters</h2>
            <p>Refine the MVP list by type, location, lifecycle state, or free-text search.</p>
          </div>
        </div>

        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="inventoryFilterEntityType">Entity type</label>
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
              <option value="">All entity types</option>
              {metadata.entityTypes.map((entityType) => (
                <option key={entityType.id} value={entityType.id}>
                  {entityType.name}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterRegion">Region</label>
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
              <option value="">All regions</option>
              {metadata.regions.map((region) => (
                <option key={region.id} value={region.id}>
                  {region.name}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterSite">Site</label>
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
              <option value="">All sites</option>
              {availableSites.map((site) => (
                <option key={site.id} value={site.id}>
                  {site.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="inventoryFilterStatus">Status</label>
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
              <option value="">All statuses</option>
              {metadata.statuses.map((status) => (
                <option key={status.code} value={status.code}>
                  {status.label}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterActivity">Lifecycle state</label>
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
              <option value="">All items</option>
              <option value="true">Active only</option>
              <option value="false">Inactive only</option>
            </select>
          </div>

          <div className="field">
            <label htmlFor="inventoryFilterSearch">Search</label>
            <input
              id="inventoryFilterSearch"
              type="text"
              value={filters.search}
              placeholder="Display name"
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
          <h2>No inventory items match the current filters.</h2>
          <p>Create the first item or loosen the filters to review the catalog.</p>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">Display name</th>
                <th scope="col">Entity type</th>
                <th scope="col">Region</th>
                <th scope="col">Site</th>
                <th scope="col">Status</th>
                <th scope="col">Lifecycle</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {inventoryItems.map((inventoryItem) => (
                <tr key={inventoryItem.id}>
                  <td>
                    <strong>{inventoryItem.displayName}</strong>
                    {inventoryItem.installationDate ? (
                      <small>Installed {inventoryItem.installationDate}</small>
                    ) : null}
                  </td>
                  <td>{inventoryItem.entityTypeName}</td>
                  <td>{inventoryItem.regionName}</td>
                  <td>{inventoryItem.siteName}</td>
                  <td>{inventoryItem.status}</td>
                  <td>
                    <span
                      className={`status-chip${
                        inventoryItem.isActive ? '' : ' status-chip--inactive'
                      }`}
                    >
                      {inventoryItem.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>
                    <div className="inline-actions">
                      <Link className="button--secondary" to={buildInventoryDetailPath(inventoryItem.id)}>
                        View
                      </Link>
                      {canWrite ? (
                        <Link className="button--secondary" to={buildInventoryEditPath(inventoryItem.id)}>
                          Edit
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
                          {inventoryItem.isActive ? 'Deactivate' : 'Activate'}
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
