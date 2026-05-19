export const routePaths = {
  home: '/',
  login: '/login',
  app: '/app',
  entityTypes: '/app/entity-types',
  entityTypeCreate: '/app/entity-types/new',
  entityTypeDetail: '/app/entity-types/:entityTypeId',
  entityTypeEdit: '/app/entity-types/:entityTypeId/edit',
  inventory: '/app/inventory',
  inventoryCreate: '/app/inventory/new',
  inventoryDetail: '/app/inventory/:inventoryItemId',
  inventoryEdit: '/app/inventory/:inventoryItemId/edit',
  preventiveTemplates: '/app/preventive-templates',
  preventiveTemplateCreate: '/app/preventive-templates/new',
  preventiveTemplateDetail: '/app/preventive-templates/:preventiveTemplateId',
  preventiveTemplateEdit: '/app/preventive-templates/:preventiveTemplateId/edit',
  preventiveExecutions: '/app/preventive-executions',
  preventiveExecutionCreate: '/app/preventive-executions/new',
  preventiveExecutionDetail: '/app/preventive-executions/:preventiveExecutionId',
  preventiveExecutionEdit: '/app/preventive-executions/:preventiveExecutionId/edit',
  preventiveValidations: '/app/preventive-validations',
  preventiveValidationDetail: '/app/preventive-validations/:preventiveExecutionId',
} as const

function requireRoutePathId(value: string, label: string) {
  if (!value || value === 'null' || value === 'undefined') {
    throw new Error(`${label} is required before building a route.`)
  }

  return encodeURIComponent(value)
}

export function buildEntityTypeDetailPath(entityTypeId: string) {
  return `/app/entity-types/${requireRoutePathId(entityTypeId, 'Entity type id')}`
}

export function buildEntityTypeEditPath(entityTypeId: string) {
  return `/app/entity-types/${requireRoutePathId(entityTypeId, 'Entity type id')}/edit`
}

export function buildInventoryDetailPath(inventoryItemId: string) {
  return `/app/inventory/${requireRoutePathId(inventoryItemId, 'Inventory item id')}`
}

export function buildInventoryEditPath(inventoryItemId: string) {
  return `/app/inventory/${requireRoutePathId(inventoryItemId, 'Inventory item id')}/edit`
}

export function buildPreventiveTemplateDetailPath(preventiveTemplateId: string) {
  return `/app/preventive-templates/${requireRoutePathId(preventiveTemplateId, 'Preventive template id')}`
}

export function buildPreventiveTemplateEditPath(preventiveTemplateId: string) {
  return `/app/preventive-templates/${requireRoutePathId(preventiveTemplateId, 'Preventive template id')}/edit`
}

export function buildPreventiveExecutionDetailPath(preventiveExecutionId: string) {
  return `/app/preventive-executions/${requireRoutePathId(preventiveExecutionId, 'Preventive execution id')}`
}

export function buildPreventiveExecutionEditPath(preventiveExecutionId: string) {
  return `/app/preventive-executions/${requireRoutePathId(preventiveExecutionId, 'Preventive execution id')}/edit`
}

export function buildPreventiveValidationDetailPath(preventiveExecutionId: string) {
  return `/app/preventive-validations/${requireRoutePathId(preventiveExecutionId, 'Preventive execution id')}`
}
