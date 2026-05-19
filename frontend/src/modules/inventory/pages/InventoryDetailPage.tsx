import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { AuditMetadata } from '@/components/audit/AuditMetadata'
import { StatusBadge } from '@/components/status/StatusBadge'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import { buildInventoryEditPath, routePaths } from '@/shared/routing/route-paths'

export function InventoryDetailPage() {
  const { inventoryItemId } = useParams()
  const queryClient = useQueryClient()
  const { hasPermission, session } = useAuthSession()
  const accessToken = session?.tokens.accessToken
  const canWrite = hasPermission(permissionCodes.inventoryWrite)

  const inventoryItemQuery = useQuery({
    queryKey: inventoryItemId ? inventoryQueryKeys.detail(inventoryItemId) : inventoryQueryKeys.all,
    queryFn: () => inventoryClient.getById(inventoryItemId!, accessToken!),
    enabled: Boolean(inventoryItemId && accessToken),
  })

  const activateMutation = useMutation({
    mutationFn: () => inventoryClient.activate(inventoryItemId!, accessToken!),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.lists() }),
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.detail(inventoryItemId!) }),
      ])
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: () => inventoryClient.deactivate(inventoryItemId!, accessToken!),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.lists() }),
        queryClient.invalidateQueries({ queryKey: inventoryQueryKeys.detail(inventoryItemId!) }),
      ])
    },
  })

  if (!inventoryItemId || !accessToken) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Inventory item could not be resolved.</h1>
        <p>The detail route requires a valid item id and an authenticated session.</p>
      </section>
    )
  }

  if (inventoryItemQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Loading inventory item.</h1>
        <p>InfraOps is fetching the stored metadata and dynamic attributes for review.</p>
      </section>
    )
  }

  if (inventoryItemQuery.isError || !inventoryItemQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Inventory</p>
        <h1>Inventory item could not be loaded.</h1>
        <p>{inventoryItemQuery.error?.message ?? 'The inventory item was not found.'}</p>
      </section>
    )
  }

  const inventoryItem = inventoryItemQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Inventory management</p>
          <h1>{inventoryItem.displayName}</h1>
        </div>
        <p>
          Review the stored inventory metadata and dynamic attribute values that
          future preventive modules will reference.
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.inventory}>
          Back to list
        </Link>
        {canWrite ? (
          <Link className="button--secondary" to={buildInventoryEditPath(inventoryItem.id)}>
            Edit item
          </Link>
        ) : null}
        {canWrite ? (
          <button
            className="button"
            type="button"
            onClick={() => {
              if (inventoryItem.isActive) {
                void deactivateMutation.mutateAsync()
                return
              }

              void activateMutation.mutateAsync()
            }}
          >
            {inventoryItem.isActive ? 'Deactivate item' : 'Activate item'}
          </button>
        ) : null}
      </div>

      <div className="detail-grid">
        <article className="card">
          <h2>Metadata</h2>
          <dl className="definition-list">
            <div>
              <dt>Entity type</dt>
              <dd>
                {inventoryItem.entityTypeName} <code>{inventoryItem.entityTypeCode}</code>
              </dd>
            </div>
            <div>
              <dt>Region</dt>
              <dd>{inventoryItem.regionName}</dd>
            </div>
            <div>
              <dt>Site</dt>
              <dd>{inventoryItem.siteName}</dd>
            </div>
            <div>
              <dt>Status</dt>
              <dd>
                <StatusBadge value={inventoryItem.status} />
              </dd>
            </div>
            <div>
              <dt>Lifecycle</dt>
              <dd>
                <StatusBadge value={inventoryItem.isActive ? 'active' : 'inactive'} />
              </dd>
            </div>
            <div>
              <dt>Installation date</dt>
              <dd>{inventoryItem.installationDate ?? 'Not defined'}</dd>
            </div>
          </dl>
        </article>
        <AuditMetadata
          createdAtUtc={inventoryItem.createdAtUtc}
          createdBy={inventoryItem.createdBy}
          updatedAtUtc={inventoryItem.updatedAtUtc}
          updatedBy={inventoryItem.updatedBy}
        />
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>Dynamic attributes</h2>
            <p>Values are stored against the active field definitions from the selected entity type.</p>
          </div>
        </div>

        {inventoryItem.attributeValues.length === 0 ? (
          <div className="empty-state">
            <h2>No dynamic attributes are stored.</h2>
            <p>This inventory item currently relies on fixed metadata only.</p>
          </div>
        ) : (
          <div className="dynamic-field-list">
            {inventoryItem.attributeValues.map((attributeValue) => (
              <article className="dynamic-field-card" key={attributeValue.fieldKey}>
                <dl className="definition-list">
                  <div>
                    <dt>{attributeValue.displayLabel}</dt>
                    <dd>{attributeValue.value}</dd>
                  </div>
                  <div>
                    <dt>Field key</dt>
                    <dd>
                      <code>{attributeValue.fieldKey}</code>
                    </dd>
                  </div>
                  <div>
                    <dt>Field type</dt>
                    <dd>{attributeValue.fieldType}</dd>
                  </div>
                </dl>
              </article>
            ))}
          </div>
        )}
      </section>
    </section>
  )
}
