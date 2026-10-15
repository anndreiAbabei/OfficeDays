import { test, expect } from '@playwright/test';

test('team day persists, outlines the complete column in both themes, and clears', async ({ page }, testInfo) => {
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill('Admin');
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.locator('.calendar')).toBeVisible();
  await page.goto('/profile');
  await page.getByLabel('Team day').selectOption('thursday');
  await page.getByRole('button', { name: 'Save profile' }).click();
  await expect(page.getByRole('status')).toHaveText('Profile saved.');
  await page.goto('/');
  const column = page.locator('.team-column');
  await expect(column).toBeVisible();
  await expect(column).toHaveCSS('grid-column-start', '4');
  for (const colorScheme of ['light', 'dark'] as const) {
    await page.emulateMedia({ colorScheme });
    const calendarBox = await page.locator('.calendar').boundingBox();
    const columnBox = await column.boundingBox();
    expect(columnBox!.height).toBeGreaterThanOrEqual(calendarBox!.height);
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
    await page.screenshot({ path: testInfo.outputPath(`team-day-${colorScheme}.png`), fullPage: true });
  }
  await page.reload();
  await expect(column).toBeVisible();
  await page.goto('/profile');
  await expect(page.getByLabel('Team day')).toHaveValue('thursday');
  await page.getByLabel('Team day').selectOption({ label: 'None' });
  await page.getByRole('button', { name: 'Save profile' }).click();
  await expect(page.getByRole('status')).toHaveText('Profile saved.');
  await page.goto('/');
  await expect(page.locator('.calendar')).toBeVisible();
  await expect(column).toHaveCount(0);
  await expect(page.getByText('Team day', { exact: true })).toHaveCount(0);
});
