import { expect, test } from '@playwright/test'
import { demoUsers } from './fixtures/users'
import { expectNoPageHorizontalOverflow, login, signOut } from './support/app'

test('admin can sign in, see the dashboard, and sign out', async ({ page }) => {
  await login(page, demoUsers.admin)

  await expect(page.getByRole('button', { name: /switch to dark theme/i })).toBeVisible()
  await expect(page.getByLabel(/language/i)).toBeVisible()
  await expect(page.getByLabel(/assigned roles/i)).toContainText(/admin/i)

  await signOut(page)
})

test('dashboard shows metrics, supports Portuguese, and persists dark theme', async ({ page }) => {
  await login(page, demoUsers.admin)

  await expect(page.getByText('Total inventory')).toBeVisible()
  await expect(page.getByText('Validation results by status')).toBeVisible()
  await expect(page.getByRole('link', { name: /open validation queue/i })).toBeVisible()

  await page.getByLabel(/language/i).selectOption('pt-BR')
  await expect(page.getByRole('heading', { name: /visão geral da manutenção preventiva/i })).toBeVisible()
  await expect(page.getByText('Inventário total')).toBeVisible()

  await page.getByRole('button', { name: /mudar para tema escuro/i }).click()
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark')

  await page.reload()
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark')
  await expect(page.getByText('Inventário total')).toBeVisible()
})

test('mobile dashboard keeps navigation and header controls usable', async ({ page }) => {
  await page.setViewportSize({ width: 360, height: 800 })
  await login(page, demoUsers.admin)

  await expect(page.getByRole('heading', { name: /infrastructure preventive maintenance overview/i })).toBeVisible()
  await expect(page.getByRole('navigation', { name: /application navigation/i })).toBeVisible()
  await expect(page.getByLabel(/language/i)).toBeVisible()
  await expect(page.getByRole('button', { name: /switch to dark theme/i })).toBeVisible()
  await expectNoPageHorizontalOverflow(page)
})
