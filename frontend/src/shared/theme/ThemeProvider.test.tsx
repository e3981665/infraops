import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { ThemeProvider } from '@/shared/theme/ThemeProvider'
import { useTheme } from '@/shared/theme/useTheme'

describe('ThemeProvider', () => {
  it('should persist selected theme and update the document theme', async () => {
    const user = userEvent.setup()

    render(
      <ThemeProvider>
        <ThemeProbe />
      </ThemeProvider>,
    )

    await user.click(screen.getByRole('button', { name: /toggle theme/i }))

    expect(window.localStorage.getItem('infraops.theme')).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
  })
})

function ThemeProbe() {
  const { theme, toggleTheme } = useTheme()

  return (
    <button type="button" onClick={toggleTheme}>
      Toggle theme {theme}
    </button>
  )
}
