import type { AuthTokens } from '@/modules/auth/types/auth-tokens'
import type { CurrentUser } from '@/modules/auth/types/current-user'

export type AuthStatus = 'loading' | 'anonymous' | 'authenticated'

export interface AuthSession {
  currentUser: CurrentUser
  tokens: AuthTokens
}
