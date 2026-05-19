import { useCallback, useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import {
  ThemeContext,
  type ThemeContextValue,
  type ThemeMode,
} from '@/shared/theme/theme-context'

const themeStorageKey = 'infraops.theme'

export function ThemeProvider({ children }: PropsWithChildren) {
  const [theme, setThemeState] = useState<ThemeMode>(() => resolveInitialTheme())

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    document.documentElement.style.colorScheme = theme
    window.localStorage.setItem(themeStorageKey, theme)
  }, [theme])

  const setTheme = useCallback((nextTheme: ThemeMode) => {
    setThemeState(nextTheme)
  }, [])

  const toggleTheme = useCallback(() => {
    setThemeState((currentTheme) => (currentTheme === 'dark' ? 'light' : 'dark'))
  }, [])

  const value = useMemo<ThemeContextValue>(
    () => ({
      theme,
      setTheme,
      toggleTheme,
    }),
    [setTheme, theme, toggleTheme],
  )

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}

function resolveInitialTheme(): ThemeMode {
  const savedTheme = window.localStorage.getItem(themeStorageKey)

  if (savedTheme === 'light' || savedTheme === 'dark') {
    return savedTheme
  }

  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}
