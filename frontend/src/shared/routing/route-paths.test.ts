import { describe, expect, it } from 'vitest'
import {
  buildInventoryDetailPath,
  buildPreventiveExecutionEditPath,
} from '@/shared/routing/route-paths'

describe('route path builders', () => {
  it('should reject invalid ids before building navigation paths', () => {
    expect(() => buildPreventiveExecutionEditPath('null')).toThrow(
      'Preventive execution id is required before building a route.',
    )
  })

  it('should encode valid ids in route paths', () => {
    expect(buildInventoryDetailPath('asset/with space')).toBe('/app/inventory/asset%2Fwith%20space')
  })
})
