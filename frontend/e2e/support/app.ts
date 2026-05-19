import { expect, type Locator, type Page } from '@playwright/test'

type DemoUser = {
  email: string
  password: string
}

export function uniqueSuffix() {
  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`
}

export async function resetBrowserState(page: Page) {
  await page.goto('/')
  await page.evaluate(() => {
    window.localStorage.clear()
    window.sessionStorage.clear()
  })
}

export async function login(page: Page, user: DemoUser) {
  await resetBrowserState(page)
  await page.goto('/login')
  await page.getByLabel('Email').fill(user.email)
  await page.getByLabel('Password').fill(user.password)
  await page.getByRole('button', { name: /sign in/i }).click()

  await expect(page).toHaveURL(/\/app\/?$/)
  await expect(page.getByText(user.email)).toBeVisible()
  await expect(page.getByRole('heading', { name: /infrastructure preventive maintenance overview/i })).toBeVisible()
}

export async function signOut(page: Page) {
  await page.getByRole('button', { name: /sign out/i }).click()
  await expect(page.getByRole('heading', { name: /sign in to infraops/i })).toBeVisible()
}

export async function navigateBySidebar(page: Page, name: RegExp | string) {
  const accessibleName =
    typeof name === 'string' ? new RegExp(`^${escapeRegExp(name)}\\b`, 'i') : name

  await page
    .getByRole('navigation', { name: /application navigation/i })
    .getByRole('link', { name: accessibleName })
    .click()
}

export async function expectNoPageHorizontalOverflow(page: Page) {
  const overflow = await page.evaluate(() => {
    const root = document.documentElement
    return root.scrollWidth - root.clientWidth
  })

  expect(overflow).toBeLessThanOrEqual(1)
}

export async function selectFirstMatchingOption(select: Locator, namePattern: RegExp) {
  await expect(select.locator('option').filter({ hasText: namePattern }).first()).toHaveCount(1)

  const options = await select.locator('option').evaluateAll((nodes) =>
    nodes.map((node) => ({
      label: node.textContent?.trim() ?? '',
      value: (node as HTMLOptionElement).value,
    })),
  )
  const option = options.find((candidate) => namePattern.test(candidate.label))

  if (!option) {
    throw new Error(`Could not find select option matching ${namePattern}`)
  }

  await select.selectOption(option.value)

  return option
}

function escapeRegExp(value: string) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}
