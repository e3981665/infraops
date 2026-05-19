import { useContext } from 'react'
import { AuthSessionContext } from '@/modules/auth/context/auth-session-context'

export function useAuthSession() {
  const context = useContext(AuthSessionContext)

  if (!context) {
    throw new Error('useAuthSession must be used within AuthSessionProvider.')
  }

  return context
}
