import type { PropsWithChildren } from 'react'
import { useCallback, useEffect, useState } from 'react'
import { ApiError } from '@/shared/api/http-client'
import {
  authClient,
  type SignInRequest,
} from '@/modules/auth/api/auth-client'
import {
  AuthSessionContext,
  type AuthSessionContextValue,
} from '@/modules/auth/context/auth-session-context'
import { authTokenStorage } from '@/modules/auth/storage/auth-token-storage'
import type { AuthSession, AuthStatus } from '@/modules/auth/types/auth-session'
import type { AuthTokens } from '@/modules/auth/types/auth-tokens'

export function AuthSessionProvider({ children }: PropsWithChildren) {
  const [status, setStatus] = useState<AuthStatus>(
    authTokenStorage.read() ? 'loading' : 'anonymous',
  )
  const [session, setSession] = useState<AuthSession | null>(null)

  const clearSession = useCallback(() => {
    authTokenStorage.clear()
    setSession(null)
    setStatus('anonymous')
  }, [])

  const establishSession = useCallback(async (tokens: AuthTokens) => {
    const currentUser = await authClient.getCurrentUser(tokens.accessToken)

    authTokenStorage.write(tokens)
    setSession({
      currentUser,
      tokens,
    })
    setStatus('authenticated')
  }, [])

  useEffect(() => {
    async function restoreSession() {
      const storedTokens = authTokenStorage.read()

      if (!storedTokens) {
        setStatus('anonymous')
        return
      }

      try {
        await establishSession(storedTokens)
      } catch (error) {
        if (error instanceof ApiError && error.status === 401) {
          try {
            const refreshedTokens = await authClient.refresh(storedTokens.refreshToken)
            await establishSession(refreshedTokens)
            return
          } catch {
            clearSession()
            return
          }
        }

        clearSession()
      }
    }

    void restoreSession()
  }, [clearSession, establishSession])

  async function signIn(requestPayload: SignInRequest) {
    const tokens = await authClient.login(requestPayload)

    try {
      await establishSession(tokens)
    } catch (error) {
      clearSession()
      throw error
    }
  }

  async function signOut() {
    const refreshToken = session?.tokens.refreshToken

    clearSession()

    if (!refreshToken) {
      return
    }

    try {
      await authClient.logout(refreshToken)
    } catch {
      // The local session is already cleared, so logout can fail quietly here.
    }
  }

  const value: AuthSessionContextValue = {
    status,
    session,
    isAuthenticated: status === 'authenticated',
    signIn,
    signOut,
    hasPermission: (permissionCode) =>
      session?.currentUser.permissions.includes(permissionCode) ?? false,
  }

  return (
    <AuthSessionContext.Provider value={value}>
      {children}
    </AuthSessionContext.Provider>
  )
}
