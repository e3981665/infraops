import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { HomePage } from '@/modules/home/pages/HomePage'

describe('HomePage', () => {
  it('should render the product overview', () => {
    renderWithProviders(<HomePage />)

    expect(
      screen.getByRole('heading', {
        name: /infrastructure operations, prepared for clean growth/i,
      }),
    ).toBeInTheDocument()
  })
})
