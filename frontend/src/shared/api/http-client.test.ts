import { beforeEach, describe, expect, it, vi } from 'vitest'
import { request } from '@/shared/api/http-client'

describe('http-client', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('should include correlation id from API error responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          code: 'server_error',
          message: 'An unexpected error occurred.',
          correlationId: 'correlation-123',
        }),
        {
          status: 500,
          headers: {
            'content-type': 'application/json',
          },
        },
      ),
    )

    await expect(request('/api/example')).rejects.toMatchObject({
      status: 500,
      code: 'server_error',
      correlationId: 'correlation-123',
      message: 'An unexpected error occurred. Reference: correlation-123',
    })
  })
})
