import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'

export function usePermission(permissionCode: string) {
  const { hasPermission } = useAuthSession()

  return hasPermission(permissionCode)
}
