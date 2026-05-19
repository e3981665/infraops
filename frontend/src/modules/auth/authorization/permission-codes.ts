export const permissionCodes = {
  usersRead: 'users.read',
  usersWrite: 'users.write',
  rolesRead: 'roles.read',
  rolesWrite: 'roles.write',
  inventoryRead: 'inventory.read',
  inventoryWrite: 'inventory.write',
  preventiveTemplatesRead: 'preventive.templates.read',
  preventiveTemplatesWrite: 'preventive.templates.write',
  preventiveRead: 'preventive.read',
  preventiveExecute: 'preventive.execute',
  preventiveValidate: 'preventive.validate',
  entityManage: 'entity.manage',
} as const

export type PermissionCode =
  (typeof permissionCodes)[keyof typeof permissionCodes]
