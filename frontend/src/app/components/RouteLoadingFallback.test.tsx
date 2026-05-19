import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { RouteLoadingFallback } from '@/app/components/RouteLoadingFallback'

describe('RouteLoadingFallback', () => {
  it('should render a stable loading state for lazy route chunks', () => {
    render(<RouteLoadingFallback />)

    expect(screen.getByRole('heading', { name: 'Loading workspace view.' })).toBeInTheDocument()
  })
})
