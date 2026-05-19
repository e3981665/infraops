import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { entityTypeQueryKeys } from '@/modules/entity-types/api/entity-type-query-keys'
import { entityTypesClient } from '@/modules/entity-types/api/entity-types-client'
import { buildEntityTypeEditPath, routePaths } from '@/shared/routing/route-paths'

export function EntityTypeDetailPage() {
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
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Entity type could not be resolved.</h1>
        <p>The detail route requires a valid entity type id and an authenticated session.</p>
      </section>
    )
  }

  if (entityTypeQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Loading entity definition.</h1>
        <p>InfraOps is fetching the current configuration for review.</p>
      </section>
    )
  }

  if (entityTypeQuery.isError || !entityTypeQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity type administration</p>
        <h1>Entity definition could not be loaded.</h1>
        <p>{entityTypeQuery.error?.message ?? 'The entity type was not found.'}</p>
      </section>
    )
  }

  const entityType = entityTypeQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Entity type administration</p>
          <h1>{entityType.name}</h1>
        </div>
        <p>
          Review the stable definition that downstream inventory and preventive
          modules will reuse.
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.entityTypes}>
          Back to list
        </Link>
        <Link className="button--secondary" to={buildEntityTypeEditPath(entityType.id)}>
          Edit definition
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
          {entityType.isActive ? 'Deactivate entity type' : 'Activate entity type'}
        </button>
      </div>

      <div className="detail-grid">
        <article className="card">
          <h2>Metadata</h2>
          <dl className="definition-list">
            <div>
              <dt>Code</dt>
              <dd>
                <code>{entityType.code}</code>
              </dd>
            </div>
            <div>
              <dt>Status</dt>
              <dd>{entityType.isActive ? 'Active' : 'Inactive'}</dd>
            </div>
            <div>
              <dt>Field count</dt>
              <dd>{entityType.fieldDefinitions.filter((field) => field.isActive).length}</dd>
            </div>
            <div>
              <dt>Description</dt>
              <dd>{entityType.description || 'No description provided.'}</dd>
            </div>
          </dl>
        </article>
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>Dynamic fields</h2>
            <p>Active and inactive field definitions remain visible for admin review.</p>
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
                  {fieldDefinition.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>

              <dl className="definition-list definition-list--compact">
                <div>
                  <dt>Field type</dt>
                  <dd>{fieldDefinition.fieldType}</dd>
                </div>
                <div>
                  <dt>Display order</dt>
                  <dd>{fieldDefinition.displayOrder}</dd>
                </div>
                <div>
                  <dt>Required</dt>
                  <dd>{fieldDefinition.isRequired ? 'Yes' : 'No'}</dd>
                </div>
                <div>
                  <dt>Placeholder</dt>
                  <dd>{fieldDefinition.placeholder || 'Not defined'}</dd>
                </div>
                <div>
                  <dt>Help text</dt>
                  <dd>{fieldDefinition.helpText || 'Not defined'}</dd>
                </div>
              </dl>

              {fieldDefinition.fieldType === 'select' ? (
                <div className="select-options-panel">
                  <div className="form-section__heading">
                    <div>
                      <h4>Select options</h4>
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
                        <span>Order {option.displayOrder}</span>
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
