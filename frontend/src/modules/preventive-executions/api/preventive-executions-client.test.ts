import { describe, expect, it, vi } from 'vitest'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'

describe('preventiveExecutionsClient', () => {
  it('should reject missing execution ids before submit reaches fetch', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch')

    expect(() => preventiveExecutionsClient.submit('null', [], 'access-token')).toThrow(
      'Preventive execution id is required before calling the API.',
    )

    expect(fetchSpy).not.toHaveBeenCalled()
  })
})
