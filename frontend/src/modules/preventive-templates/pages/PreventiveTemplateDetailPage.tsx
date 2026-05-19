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

export function PreventiveTemplateDetailPage() {
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
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Preventive template could not be resolved.</h1>
        <p>The detail route requires a valid preventive template id and an authenticated session.</p>
      </section>
    )
  }

  if (templateQuery.isLoading) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Loading preventive template definition.</h1>
        <p>InfraOps is fetching the current checklist structure for review.</p>
      </section>
    )
  }

  if (templateQuery.isError || !templateQuery.data) {
    return (
      <section className="status-panel">
        <p className="hero-panel__eyebrow">Preventive template administration</p>
        <h1>Preventive template could not be loaded.</h1>
        <p>{templateQuery.error?.message ?? 'The preventive template was not found.'}</p>
      </section>
    )
  }

  const template = templateQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Preventive template administration</p>
          <h1>{template.name}</h1>
        </div>
        <p>
          Review the active and inactive checklist structure that preventive execution will later
          consume.
        </p>
      </div>

      <div className="module-panel__actions">
        <Link className="button--secondary" to={routePaths.preventiveTemplates}>
          Back to list
        </Link>
        {canWrite ? (
          <Link className="button--secondary" to={buildPreventiveTemplateEditPath(template.id)}>
            Edit template
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
            {template.isActive ? 'Deactivate template' : 'Activate template'}
          </button>
        ) : null}
      </div>

      <div className="detail-grid">
        <article className="card">
          <h2>Metadata</h2>
          <dl className="definition-list">
            <div>
              <dt>Entity type</dt>
              <dd>{template.entityTypeName}</dd>
            </div>
            <div>
              <dt>Code</dt>
              <dd>
                <code>{template.code}</code>
              </dd>
            </div>
            <div>
              <dt>Status</dt>
              <dd>{template.isActive ? 'Active' : 'Inactive'}</dd>
            </div>
            <div>
              <dt>Sections</dt>
              <dd>{template.sections.filter((section) => section.isActive).length}</dd>
            </div>
            <div>
              <dt>Description</dt>
              <dd>{template.description || 'No description provided.'}</dd>
            </div>
          </dl>
        </article>
      </div>

      <section className="form-section">
        <div className="form-section__heading">
          <div>
            <h2>Sections and checklist items</h2>
            <p>Inactive sections and items remain visible for audit-friendly admin review.</p>
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
                  <h3>{section.title}</h3>
                  <p>Display order {section.displayOrder}</p>
                </div>
                <span className={`status-chip${section.isActive ? '' : ' status-chip--inactive'}`}>
                  {section.isActive ? 'Active' : 'Inactive'}
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
                        <h4>{item.label}</h4>
                        <p>
                          <code>{item.itemKey}</code>
                        </p>
                      </div>
                      <span className={`status-chip${item.isActive ? '' : ' status-chip--inactive'}`}>
                        {item.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>

                    <dl className="definition-list definition-list--compact">
                      <div>
                        <dt>Item type</dt>
                        <dd>{item.itemType}</dd>
                      </div>
                      <div>
                        <dt>Display order</dt>
                        <dd>{item.displayOrder}</dd>
                      </div>
                      <div>
                        <dt>Required</dt>
                        <dd>{item.isRequired ? 'Yes' : 'No'}</dd>
                      </div>
                      <div>
                        <dt>Critical</dt>
                        <dd>{item.isCritical ? 'Yes' : 'No'}</dd>
                      </div>
                      <div>
                        <dt>Comment on failure</dt>
                        <dd>{item.requiresCommentOnFailure ? 'Yes' : 'No'}</dd>
                      </div>
                      <div>
                        <dt>Photo on failure</dt>
                        <dd>{item.requiresPhotoOnFailure ? 'Yes' : 'No'}</dd>
                      </div>
                      <div>
                        <dt>Help text</dt>
                        <dd>{item.helpText || 'Not defined'}</dd>
                      </div>
                      {item.itemType === 'numeric' ? (
                        <>
                          <div>
                            <dt>Minimum value</dt>
                            <dd>{item.minimumValue ?? 'Not defined'}</dd>
                          </div>
                          <div>
                            <dt>Maximum value</dt>
                            <dd>{item.maximumValue ?? 'Not defined'}</dd>
                          </div>
                        </>
                      ) : null}
                    </dl>

                    {item.itemType === 'select' ? (
                      <div className="select-options-panel">
                        <div className="form-section__heading">
                          <div>
                            <h4>Select options</h4>
                          </div>
                        </div>
                        <div className="entity-field-option-list">
                          {item.options.map((option) => (
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
            </article>
          ))}
        </div>
      </section>
    </section>
  )
}
