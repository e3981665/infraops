import { request } from '@/shared/api/http-client'
import type { AuthTokens } from '@/modules/auth/types/auth-tokens'
import type { CurrentUser } from '@/modules/auth/types/current-user'

export interface SignInRequest {
  email: string
  password: string
}

interface RefreshTokenRequest {
  refreshToken: string
}

export const authClient = {
  login(requestPayload: SignInRequest) {
    return request<AuthTokens>('/api/auth/login', {
      method: 'POST',
      body: requestPayload,
    })
  },
  refresh(refreshToken: string) {
    return request<AuthTokens>('/api/auth/refresh', {
      method: 'POST',
      body: { refreshToken } satisfies RefreshTokenRequest,
    })
  },
  logout(refreshToken: string) {
    return request<void>('/api/auth/logout', {
      method: 'POST',
      body: { refreshToken } satisfies RefreshTokenRequest,
    })
  },
  getCurrentUser(accessToken: string) {
    return request<CurrentUser>('/api/auth/me', {
      accessToken,
    })
  },
}
