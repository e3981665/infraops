import { environment } from '@/shared/config/environment'

export function buildApiUrl(path: string) {
  return new URL(path, environment.apiBaseUrl).toString()
}

export function requireApiPathId(value: string, label: string) {
  if (!value || value === 'null' || value === 'undefined') {
    throw new Error(`${label} is required before calling the API.`)
  }

  return encodeURIComponent(value)
}

export interface ApiErrorResponse {
  code: string
  message: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly errors?: Record<string, string[]>

  constructor(status: number, response: ApiErrorResponse) {
    super(response.message)
    this.name = 'ApiError'
    this.status = status
    this.code = response.code
    this.errors = response.errors
  }
}

export interface RequestOptions extends Omit<RequestInit, 'body'> {
  accessToken?: string
  body?: BodyInit | object | null
}

export async function request<TResponse>(
  path: string,
  options?: RequestOptions,
): Promise<TResponse> {
  const headers = new Headers(options?.headers)

  if (options?.accessToken) {
    headers.set('Authorization', `Bearer ${options.accessToken}`)
  }

  let body = options?.body

  if (body && typeof body === 'object' && !(body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
    body = JSON.stringify(body)
  }

  const response = await fetch(buildApiUrl(path), {
    ...options,
    headers,
    body: body as BodyInit | null | undefined,
  })

  if (response.status === 204) {
    return undefined as TResponse
  }

  const contentType = response.headers.get('content-type') ?? ''
  const hasJsonBody = contentType.includes('application/json')

  if (!response.ok) {
    if (hasJsonBody) {
      throw new ApiError(response.status, (await response.json()) as ApiErrorResponse)
    }

    throw new ApiError(response.status, {
      code: 'http_error',
      message: `Request failed with status ${response.status}.`,
    })
  }

  if (!hasJsonBody) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}
