import { expect, test, type Page } from '@playwright/test'
import { demoUsers } from './fixtures/users'
import { login, navigateBySidebar, selectFirstMatchingOption, uniqueSuffix } from './support/app'

async function fillBuilderInput(page: Page, inputId: string, value: string) {
  const input = page.locator(`[id="${inputId}"]`)

  await expect(input).toBeVisible()
  await input.click()
  await input.pressSequentially(value)
  await expect(input).toHaveValue(value)
}

test('admin creates an entity type with dynamic text and select fields', async ({ page }) => {
  const suffix = uniqueSuffix()
  const entityName = `E2E Asset ${suffix}`
  const entityCode = `e2e-asset-${suffix}`

  await login(page, demoUsers.admin)
  await navigateBySidebar(page, 'Entity Types')
  await page.getByRole('link', { name: /create entity type/i }).click()

  await page.getByLabel('Entity type name').fill(entityName)
  await page.getByLabel('Entity type code').fill(entityCode)
  await page.getByLabel('Description').fill('Created by Playwright E2E coverage.')

  await page.getByLabel('Field key').fill('assetTag')
  await expect(page.locator('[id="fieldDefinitions.0.fieldKey"]')).toHaveValue('assetTag')
  await fillBuilderInput(page, 'fieldDefinitions.0.displayLabel', 'Asset Tag')
  await page.getByLabel('Field type').selectOption('text')
  await page.getByLabel('Required field').check()

  await page.getByRole('button', { name: /add field/i }).click()
  await page.getByLabel('Field key').nth(1).fill('assetClass')
  await expect(page.locator('[id="fieldDefinitions.1.fieldKey"]')).toHaveValue('assetClass')
  await fillBuilderInput(page, 'fieldDefinitions.1.displayLabel', 'Asset Class')
  await page.getByLabel('Field type').nth(1).selectOption('select')

  await page.getByRole('button', { name: /add option/i }).click()
  await page.locator('[id="fieldDefinitions.1.options.0.value"]').fill('primary')
  await page.locator('[id="fieldDefinitions.1.options.0.label"]').fill('Primary')
  await page.getByRole('button', { name: /add option/i }).click()
  await page.locator('[id="fieldDefinitions.1.options.1.value"]').fill('backup')
  await page.locator('[id="fieldDefinitions.1.options.1.label"]').fill('Backup')

  await page.getByRole('button', { name: /save entity type/i }).click()

  await expect(page.getByRole('heading', { name: entityName })).toBeVisible()
  await expect(page.getByText('Asset Tag')).toBeVisible()
  await expect(page.getByText('Asset Class')).toBeVisible()

  await navigateBySidebar(page, 'Entity Types')
  await expect(page.getByText(entityName).first()).toBeVisible()
})

test('admin creates inventory with dynamic fields from a configured entity type', async ({ page }) => {
  const suffix = uniqueSuffix()
  const inventoryName = `E2E-INV-${suffix}`

  await login(page, demoUsers.admin)
  await navigateBySidebar(page, 'Inventory')
  await page.getByRole('link', { name: /register inventory item/i }).click()

  await selectFirstMatchingOption(page.getByLabel('Region'), /North Region|Região Norte/)
  await selectFirstMatchingOption(page.getByLabel('Site'), /North Hub|Hub Norte/)

  await page.getByLabel('Display name').fill(inventoryName)
  await page.getByLabel('Installation date').fill('2026-05-19')
  await selectFirstMatchingOption(page.getByLabel('Entity type'), /^Generator$/)

  await expect(page.getByLabel(/serial number/i)).toBeVisible()
  await page.getByLabel(/serial number/i).fill(`${inventoryName}-SN`)
  await page.getByLabel(/manufacturer/i).fill('Playwright Manufacturing')
  await page.getByLabel(/model/i).fill('E2E-MODEL')

  await page.getByRole('button', { name: /save inventory item/i }).click()

  await expect(page.getByRole('heading', { name: inventoryName })).toBeVisible()
  await expect(page.getByText(`${inventoryName}-SN`)).toBeVisible()

  await navigateBySidebar(page, 'Inventory')
  await page.getByLabel('Search').fill(inventoryName)
  await expect(page.getByText(inventoryName).first()).toBeVisible()
})

