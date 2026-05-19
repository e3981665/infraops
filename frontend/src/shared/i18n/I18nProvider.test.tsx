import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { I18nProvider } from '@/shared/i18n/I18nProvider'
import { useTranslation } from '@/shared/i18n/useTranslation'

describe('I18nProvider', () => {
  it('should persist selected locale and translate text', async () => {
    const user = userEvent.setup()

    render(
      <I18nProvider>
        <LocaleProbe />
      </I18nProvider>,
    )

    await user.click(screen.getByRole('button', { name: /portuguese/i }))

    expect(window.localStorage.getItem('infraops.locale')).toBe('pt-BR')
    expect(document.documentElement.lang).toBe('pt-BR')
    expect(screen.getByText('Inventário')).toBeInTheDocument()
  })
})

function LocaleProbe() {
  const { setLocale, t } = useTranslation()

  return (
    <>
      <p>{t('nav.inventory')}</p>
      <button type="button" onClick={() => setLocale('pt-BR')}>
        Portuguese
      </button>
    </>
  )
}
