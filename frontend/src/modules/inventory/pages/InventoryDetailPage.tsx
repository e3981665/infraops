import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { AuditMetadata } from '@/components/audit/AuditMetadata'
import { StatusBadge } from '@/components/status/StatusBadge'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import { buildInventoryEditPath, routePaths } from '@/shared/routing/route-paths'
import { useTranslation } from '@/shared/i18n/useTranslation'
import { localizeDemoText } from '@/shared/i18n/localized-domain-labels'

export function InventoryDetailPage() {
  const { locale, t } = useTranslation()
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
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.unresolvedTitle')}</h1>
        <p>{t('inventory.detailUnresolvedHelp')}</p>
      </section>
    )
  }

  if (inventoryItemQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.loadingItem')}</h1>
        <p>{t('inventory.detailLoadingHelp')}</p>
      </section>
    )
  }

  if (inventoryItemQuery.isError || !inventoryItemQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">{t('inventory.eyebrow')}</p>
        <h1>{t('inventory.itemLoadFailed')}</h1>
        <p>{inventoryItemQuery.error?.message ?? t('inventory.notFound')}</p>
      </section>
    )
  }

  const inventoryItem = inventoryItemQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">{t('inventory.management')}</p>
          <h1>{inventoryItem.displayName}</h1>
        </div>
        <p>
          {t('inventory.detailDescription')}
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.inventory}>
          {t('common.backToList')}
        </Link>
        {canWrite ? (
          <Link className="button--secondary" to={buildInventoryEditPath(inventoryItem.id)}>
            {t('inventory.editItem')}
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
            {inventoryItem.isActive ? t('inventory.deactivateItem') : t('inventory.activateItem')}
          </button>
        ) : null}
      </div>

      <div className="detail-grid">
        <article className="card">
          <h2>{t('common.metadata')}</h2>
          <dl className="definition-list">
            <div>
              <dt>{t('common.entityType')}</dt>
              <dd>
                {localizeDemoText(inventoryItem.entityTypeName, locale)}{' '}
                <code>{inventoryItem.entityTypeCode}</code>
              </dd>
            </div>
            <div>
              <dt>{t('common.region')}</dt>
              <dd>{localizeDemoText(inventoryItem.regionName, locale)}</dd>
            </div>
            <div>
              <dt>{t('common.site')}</dt>
              <dd>{localizeDemoText(inventoryItem.siteName, locale)}</dd>
            </div>
            <div>
              <dt>{t('common.status')}</dt>
              <dd>
                <StatusBadge value={inventoryItem.status} />
              </dd>
            </div>
            <div>
              <dt>{t('common.lifecycle')}</dt>
              <dd>
                <StatusBadge value={inventoryItem.isActive ? 'active' : 'inactive'} />
              </dd>
            </div>
            <div>
              <dt>{t('inventory.form.installationDate')}</dt>
              <dd>{inventoryItem.installationDate ?? t('common.notDefined')}</dd>
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
            <h2>{t('inventory.form.dynamicAttributes')}</h2>
            <p>{t('inventory.dynamicAttributesStoredHelp')}</p>
          </div>
        </div>

        {inventoryItem.attributeValues.length === 0 ? (
          <div className="empty-state">
            <h2>{t('inventory.noDynamicAttributes')}</h2>
            <p>{t('inventory.noDynamicAttributesHelp')}</p>
          </div>
        ) : (
          <div className="dynamic-field-list">
            {inventoryItem.attributeValues.map((attributeValue) => (
              <article className="dynamic-field-card" key={attributeValue.fieldKey}>
                <dl className="definition-list">
                  <div>
                    <dt>{localizeDemoText(attributeValue.displayLabel, locale)}</dt>
                    <dd>{attributeValue.value}</dd>
                  </div>
                  <div>
                    <dt>{t('entity.form.fieldKey')}</dt>
                    <dd>
                      <code>{attributeValue.fieldKey}</code>
                    </dd>
                  </div>
                  <div>
                    <dt>{t('entity.form.fieldType')}</dt>
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
