import { test, expect } from '@playwright/test';

test('office plans save on tap, survive reload, and respond to vacation changes', async ({ page }, testInfo) => {
  const username = `planner-${testInfo.project.name}-${Date.now()}`;
  await page.goto('/register');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByLabel('Confirm password').fill('BrowserTestsOnly!123');
  await page.getByLabel('Timezone').fill('UTC');
  await page.getByRole('combobox', { name: 'Holiday calendar' }).selectOption('RO');
  await page.getByRole('button', { name: 'Create account', exact: true }).click();
  await expect(page.getByRole('status')).toContainText('Account created');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.locator('.day[aria-current="date"]')).toBeVisible();

  const picker = page.getByLabel('Selected month');
  await picker.getByLabel('Year').fill('2099');
  await picker.getByLabel('Year').press('Tab');
  await picker.getByRole('combobox').selectOption('10');
  const calendar = page.getByRole('region', { name: 'Plan office days' });
  const cell = calendar.getByRole('button', { name: /Oct 12, 2099/ });
  const count = page.locator('.metric').filter({ hasText: 'Upcoming planned' }).locator('strong');
  await expect(cell).toBeEnabled();
  await cell.click();
  await expect(cell).toHaveClass(/planned/);
  await expect(count).toHaveText(/^1 \/ \d+$/);
  await expect(calendar).toHaveAttribute('aria-busy', 'false');
  await page.reload();
  await picker.getByLabel('Year').fill('2099');
  await picker.getByLabel('Year').press('Tab');
  await picker.getByRole('combobox').selectOption('10');
  await expect(cell).toHaveAttribute('aria-pressed', 'true');
  await expect(count).toHaveText(/^1 \/ \d+$/);

  await page.getByLabel('From', { exact: true }).fill('2099-10-12');
  await page.getByLabel('To', { exact: true }).fill('2099-10-12');
  await page.getByRole('button', { name: 'Add', exact: true }).click();
  await expect(cell).toHaveClass(/holiday/);
  await expect(count).toHaveText(/^0 \/ \d+$/);
  await cell.click();
  await expect(calendar.locator('.day-details')).toContainText('Vacation');
  await expect(cell).toHaveAttribute('aria-pressed', 'true');
  await page.getByRole('button', { name: /Remove vacation/ }).click();
  await expect(cell).toHaveClass(/planned/);
  await expect(count).toHaveText(/^1 \/ \d+$/);
  await expect(calendar).toHaveAttribute('aria-busy', 'false');
  await page.screenshot({ path: testInfo.outputPath('planning.png'), fullPage: true });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await cell.click();
  await expect(count).toHaveText(/^0 \/ \d+$/);
  await expect(cell).toHaveAttribute('aria-pressed', 'false');
});

test('two-digit planning totals stay inside the mobile card in both themes', async ({ page }, testInfo) => {
  await page.route('**/api/status?*', route => route.fulfill({ json: {
    period: '2026-10', eligibleWorkingDays: 22, maximumWfhDays: 11, requiredOfficeDays: 11,
    officeDays: 0, remainingOfficeDays: 11, upcomingPlannedDays: 11, progressPercentage: 0
  } }));
  await page.route('**/api/planning?*', route => route.fulfill({ json: {
    today: '2026-10-10', upcomingPlannedDays: 11, days: []
  } }));
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill('Admin');
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  const card = page.locator('.metric').filter({ hasText: 'Upcoming planned' });
  await expect(card.locator('strong')).toHaveText('11 / 11');
  for (const colorScheme of ['light', 'dark'] as const) {
    await page.emulateMedia({ colorScheme });
    for (const width of [320, 390, 600]) {
      await page.setViewportSize({ width, height: 850 });
      const bounds = await card.evaluate(element => {
        const card = element.getBoundingClientRect();
        const styles = getComputedStyle(element);
        const range = document.createRange();
        range.selectNodeContents(element.querySelector('strong')!);
        const ratio = range.getBoundingClientRect();
        const hint = element.querySelector('p')!.getBoundingClientRect();
        return { left: card.left + parseFloat(styles.paddingLeft), right: card.right - parseFloat(styles.paddingRight),
          ratioLeft: ratio.left, ratioRight: ratio.right, hintRight: hint.right };
      });
      expect(bounds.ratioLeft).toBeGreaterThanOrEqual(bounds.left - 1);
      expect(bounds.ratioRight).toBeLessThanOrEqual(bounds.right + 1);
      expect(bounds.hintRight).toBeLessThanOrEqual(bounds.right + 1);
    }
  }
  await card.screenshot({ path: testInfo.outputPath('planning-ratio-mobile.png') });
});
