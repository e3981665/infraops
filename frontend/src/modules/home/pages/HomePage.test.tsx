import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { HomePage } from '@/modules/home/pages/HomePage'

describe('HomePage', () => {
  it('should render the product overview', () => {
    render(
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>,
    )

    expect(
      screen.getByRole('heading', {
        name: /infrastructure operations, prepared for clean growth/i,
      }),
    ).toBeInTheDocument()
  })
})
