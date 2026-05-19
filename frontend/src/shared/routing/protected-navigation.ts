import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import type { PermissionCode } from '@/modules/auth/authorization/permission-codes'
import { routePaths } from '@/shared/routing/route-paths'

export interface ProtectedNavigationItem {
  label: string
  description: string
  to: string
  requiredPermission?: PermissionCode
}

export const protectedNavigationItems: ProtectedNavigationItem[] = [
  {
    label: 'Dashboard',
    description: 'Authentication, access, and module entry points.',
    to: routePaths.app,
  },
  {
    label: 'Entity Types',
    description: 'Admin-managed dynamic definitions that inventory will reuse.',
    to: routePaths.entityTypes,
    requiredPermission: permissionCodes.entityManage,
  },
  {
    label: 'Inventory',
    description: 'Dynamic asset registration and lifecycle management.',
    to: routePaths.inventory,
    requiredPermission: permissionCodes.inventoryRead,
  },
  {
    label: 'Preventive Templates',
    description: 'Entity-type checklist definitions that preventive execution will consume.',
    to: routePaths.preventiveTemplates,
    requiredPermission: permissionCodes.preventiveTemplatesRead,
  },
  {
    label: 'Executions',
    description: 'Start and submit preventive checklist work.',
    to: routePaths.preventiveExecutions,
    requiredPermission: permissionCodes.preventiveRead,
  },
  {
    label: 'Validations',
    description: 'Review submitted executions and record validation decisions.',
    to: routePaths.preventiveValidations,
    requiredPermission: permissionCodes.preventiveValidate,
  },
]
