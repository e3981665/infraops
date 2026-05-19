import type { AuthTokens } from '@/modules/auth/types/auth-tokens'

const storageKey = 'infraops.auth.tokens'

function isAuthTokens(value: unknown): value is AuthTokens {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Record<string, unknown>

  return (
    typeof candidate.accessToken === 'string' &&
    typeof candidate.accessTokenExpiresAtUtc === 'string' &&
    typeof candidate.refreshToken === 'string' &&
    typeof candidate.refreshTokenExpiresAtUtc === 'string'
  )
}

export const authTokenStorage = {
  read(): AuthTokens | null {
    if (typeof window === 'undefined') {
      return null
    }

    const rawValue = window.localStorage.getItem(storageKey)

    if (!rawValue) {
      return null
    }

    try {
      const parsedValue = JSON.parse(rawValue) as unknown

      if (!isAuthTokens(parsedValue)) {
        window.localStorage.removeItem(storageKey)
        return null
      }

      return parsedValue
    } catch {
      window.localStorage.removeItem(storageKey)
      return null
    }
  },
  write(tokens: AuthTokens) {
    window.localStorage.setItem(storageKey, JSON.stringify(tokens))
  },
  clear() {
    window.localStorage.removeItem(storageKey)
  },
}
