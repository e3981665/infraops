import { createContext } from 'react'
import type { AuthSession, AuthStatus } from '@/modules/auth/types/auth-session'
import type { SignInRequest } from '@/modules/auth/api/auth-client'

export interface AuthSessionContextValue {
  status: AuthStatus
  session: AuthSession | null
  isAuthenticated: boolean
  signIn: (request: SignInRequest) => Promise<void>
  signOut: () => Promise<void>
  hasPermission: (permissionCode: string) => boolean
}

export const AuthSessionContext = createContext<AuthSessionContextValue | null>(null)
