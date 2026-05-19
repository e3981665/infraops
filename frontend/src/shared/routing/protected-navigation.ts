import { permissionCodes } from '@/modules/auth/authorization/permission-codes'
import type { PermissionCode } from '@/modules/auth/authorization/permission-codes'
import type { TranslationKey } from '@/shared/i18n/translations'
import { routePaths } from '@/shared/routing/route-paths'

export interface ProtectedNavigationItem {
  labelKey: TranslationKey
  descriptionKey: TranslationKey
  to: string
  requiredPermission?: PermissionCode
}

export const protectedNavigationItems: ProtectedNavigationItem[] = [
  {
    labelKey: 'nav.dashboard',
    descriptionKey: 'nav.dashboard.description',
    to: routePaths.app,
  },
  {
    labelKey: 'nav.entityTypes',
    descriptionKey: 'nav.entityTypes.description',
    to: routePaths.entityTypes,
    requiredPermission: permissionCodes.entityManage,
  },
  {
    labelKey: 'nav.inventory',
    descriptionKey: 'nav.inventory.description',
    to: routePaths.inventory,
    requiredPermission: permissionCodes.inventoryRead,
  },
  {
    labelKey: 'nav.preventiveTemplates',
    descriptionKey: 'nav.preventiveTemplates.description',
    to: routePaths.preventiveTemplates,
    requiredPermission: permissionCodes.preventiveTemplatesRead,
  },
  {
    labelKey: 'nav.executions',
    descriptionKey: 'nav.executions.description',
    to: routePaths.preventiveExecutions,
    requiredPermission: permissionCodes.preventiveRead,
  },
  {
    labelKey: 'nav.validations',
    descriptionKey: 'nav.validations.description',
    to: routePaths.preventiveValidations,
    requiredPermission: permissionCodes.preventiveValidate,
  },
]
