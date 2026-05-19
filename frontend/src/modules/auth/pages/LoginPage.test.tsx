import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { LoginPage } from '@/modules/auth/pages/LoginPage'

describe('LoginPage', () => {
  it('should validate the login form before submission', async () => {
    const user = userEvent.setup()

    renderWithProviders(<LoginPage />, { route: '/login' })

    await user.click(screen.getByRole('button', { name: /sign in/i }))

    expect(screen.getByText(/enter a valid email address/i)).toBeInTheDocument()
    expect(
      screen.getByText(/password must contain at least 8 characters/i),
    ).toBeInTheDocument()
  })
})
