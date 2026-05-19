import { expect, test } from '@playwright/test'
import { demoUsers } from './fixtures/users'
import { login, navigateBySidebar, selectFirstMatchingOption } from './support/app'

test('technician saves a draft, submits it, and validator approves the execution', async ({ page }) => {
  await login(page, demoUsers.technician)
  await navigateBySidebar(page, 'Executions')
  await page.getByRole('link', { name: /start execution/i }).click()

  await selectFirstMatchingOption(page.getByLabel('Inventory item'), /UPS-01/)
  await expect(page.getByText('Visual Inspection')).toBeVisible()
  await expect(page.getByText('Operational Readings')).toBeVisible()

  await page.getByLabel(/equipment clean/i).selectOption('true')
  await page.getByRole('button', { name: /save draft/i }).click()
  await expect(page).toHaveURL(/\/app\/preventive-executions\/[^/]+\/edit$/)

  const executionId = page.url().match(/preventive-executions\/([^/]+)\/edit/)?.[1]
  expect(executionId).toBeTruthy()

  await page.getByLabel(/equipment clean/i).selectOption('true')
  await page.getByLabel(/any active alarm/i).selectOption('true')
  await page.getByLabel(/primary operating reading/i).fill('220')
  await page.getByLabel(/overall condition/i).selectOption('good')
  await page.getByRole('button', { name: /^submit$/i }).click()

  await expect(page.getByText('Submitted', { exact: true }).first()).toBeVisible()
  await page.getByRole('button', { name: /sign out/i }).click()
  await expect(page.getByRole('heading', { name: /sign in to infraops/i })).toBeVisible()

  await login(page, demoUsers.validator)
  await page.goto(`/app/preventive-validations/${executionId}`)
  await expect(page.getByRole('heading', { name: 'UPS-01' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Validation action' })).toBeVisible()

  await page.getByRole('button', { name: /^approve$/i }).click()
  await expect(page).toHaveURL(/\/app\/preventive-validations\?status=approved$/)
  await expect(page.getByLabel('Status')).toHaveValue('approved')
  await expect(page.locator('tbody').getByText('Approved', { exact: true }).first()).toBeVisible()
})
