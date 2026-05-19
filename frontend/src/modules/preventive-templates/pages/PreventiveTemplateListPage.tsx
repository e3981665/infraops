import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import { preventiveTemplateQueryKeys } from '@/modules/preventive-templates/api/preventive-template-query-keys'
import { preventiveTemplatesClient } from '@/modules/preventive-templates/api/preventive-templates-client'
import type { PreventiveTemplateListFilters } from '@/modules/preventive-templates/types/preventive-template'
import {
  buildPreventiveTemplateDetailPath,
  buildPreventiveTemplateEditPath,
  routePaths,
} from '@/shared/routing/route-paths'

const defaultFilters: PreventiveTemplateListFilters = {
  entityTypeId: '',
  isActive: '',
  search: '',
}

export function PreventiveTemplateListPage() {
  const queryClient = useQueryClient()
  const { hasPermission, session } = useAuthSession()
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
        <p className="hero-panel__eyebrow">Preventive templates</p>
        <h1>Authenticated access is required.</h1>
        <p>The preventive template workspace is waiting for a valid session.</p>
      </section>
    )
  }

  if (metadataQuery.isLoading || templatesQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive templates</p>
        <h1>Loading preventive templates.</h1>
        <p>InfraOps is fetching current checklist definitions and filter metadata.</p>
      </section>
    )
  }

  if (metadataQuery.isError || templatesQuery.isError || !metadataQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive templates</p>
        <h1>Preventive templates could not be loaded.</h1>
        <p>{metadataQuery.error?.message ?? templatesQuery.error?.message}</p>
      </section>
    )
  }

  const templates = templatesQuery.data ?? []

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Preventive template administration</p>
          <h1>Preventive templates by entity type.</h1>
        </div>
        <p>
          Define reusable checklist structures that future preventive execution flows will
          render dynamically.
        </p>
      </div>

      <div className="module-panel__actions">
        {canWrite ? (
          <Link className="button" to={routePaths.preventiveTemplateCreate}>
            Create preventive template
          </Link>
        ) : null}
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>Filters</h2>
            <p>Refine the list by entity type, lifecycle state, or free-text search.</p>
          </div>
        </div>

        <div className="field-grid field-grid--three-columns">
          <div className="field">
            <label htmlFor="preventiveTemplateFilterEntityType">Entity type</label>
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
              <option value="">All entity types</option>
              {metadataQuery.data.entityTypes.map((entityType) => (
                <option key={entityType.id} value={entityType.id}>
                  {entityType.name}
                </option>
              ))}
            </select>
          </div>

          <div className="field">
            <label htmlFor="preventiveTemplateFilterActivity">Lifecycle state</label>
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
              <option value="">All templates</option>
              <option value="true">Active only</option>
              <option value="false">Inactive only</option>
            </select>
          </div>

          <div className="field">
            <label htmlFor="preventiveTemplateFilterSearch">Search</label>
            <input
              id="preventiveTemplateFilterSearch"
              type="text"
              value={filters.search}
              placeholder="Template name or code"
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
          <h2>No preventive templates match the current filters.</h2>
          <p>Create the first template or loosen the filters to review the catalog.</p>
        </div>
      ) : (
        <div className="table-panel">
          <table className="data-table">
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Code</th>
                <th scope="col">Entity type</th>
                <th scope="col">Status</th>
                <th scope="col">Sections</th>
                <th scope="col">Checklist items</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {templates.map((template) => (
                <tr key={template.id}>
                  <td>
                    <strong>{template.name}</strong>
                    {template.description ? <small>{template.description}</small> : null}
                  </td>
                  <td>
                    <code>{template.code}</code>
                  </td>
                  <td>{template.entityTypeName}</td>
                  <td>
                    <span className={`status-chip${template.isActive ? '' : ' status-chip--inactive'}`}>
                      {template.isActive ? 'Active' : 'Inactive'}
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
                        View
                      </Link>
                      {canWrite ? (
                        <Link
                          className="button--secondary"
                          to={buildPreventiveTemplateEditPath(template.id)}
                        >
                          Edit
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
                          {template.isActive ? 'Deactivate' : 'Activate'}
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
