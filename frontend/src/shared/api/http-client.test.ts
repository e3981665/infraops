import { describe, expect, it } from 'vitest'
import { requireApiPathId } from '@/shared/api/http-client'

describe('requireApiPathId', () => {
  it.each(['', 'null', 'undefined'])('should reject invalid path id "%s"', (value) => {
    expect(() => requireApiPathId(value, 'Resource id')).toThrow(
      'Resource id is required before calling the API.',
    )
  })

  it('should encode valid path ids', () => {
    expect(requireApiPathId('asset/with space', 'Resource id')).toBe('asset%2Fwith%20space')
  })
})
