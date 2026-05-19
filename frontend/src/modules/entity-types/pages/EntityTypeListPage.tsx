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

export function EntityTypeListPage() {
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
        <p className="hero-panel__eyebrow">Entity types</p>
        <h1>Authenticated access is required.</h1>
        <p>The admin workspace is waiting for a valid session.</p>
      </section>
    )
  }

  if (entityTypesQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity types</p>
        <h1>Loading configurable definitions.</h1>
        <p>InfraOps is fetching the current entity-type catalog.</p>
      </section>
    )
  }

  if (entityTypesQuery.isError) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Entity types</p>
        <h1>Entity definitions could not be loaded.</h1>
        <p>{entityTypesQuery.error.message}</p>
      </section>
    )
  }

  const entityTypes = entityTypesQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Entity type administration</p>
          <h1>Configurable infrastructure definitions.</h1>
        </div>
        <p>
          Define reusable asset models and dynamic fields that Inventory will
          later render and validate.
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button" to={routePaths.entityTypeCreate}>
          Create entity type
        </Link>
      </div>

      {entityTypes.length === 0 ? (
        <div className="empty-state">
          <h2>No entity types defined yet.</h2>
          <p>Create the first definition to prepare the inventory module.</p>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Code</th>
                <th scope="col">Status</th>
                <th scope="col">Fields</th>
                <th scope="col">Actions</th>
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
                      {entityType.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>{entityType.fieldCount}</td>
                  <td>
                    <div className="inline-actions">
                      <Link className="button--secondary" to={buildEntityTypeDetailPath(entityType.id)}>
                        View
                      </Link>
                      <Link className="button--secondary" to={buildEntityTypeEditPath(entityType.id)}>
                        Edit
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
                        {entityType.isActive ? 'Deactivate' : 'Activate'}
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
