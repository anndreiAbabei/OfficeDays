import { test, expect } from '@playwright/test';
import { readFile } from 'node:fs/promises';

test('reports filter months and export the displayed snapshot until refreshed', async ({ page }, testInfo) => {
  const username = `report-${testInfo.project.name}-${Date.now()}`;
  const registration = await page.request.post('/api/users', { data: { username, password: 'BrowserTestsOnly!123', timeZoneId: 'UTC', countryCode: 'RO' } });
  expect(registration.ok()).toBe(true);
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  const responsePromise = page.waitForResponse(response => response.url().includes('/api/reports?') && response.ok());
  await page.getByRole('link', { name: 'Reports', exact: true }).click();
  const snapshot = await (await responsePromise).json();
  await expect(page.getByRole('table')).toBeVisible();
  const links = await page.getByRole('navigation', { name: 'Main navigation' }).getByRole('link').allTextContents();
  expect(links.indexOf('Reports')).toBe(links.indexOf('Profile') + 1);
  const firstMonth = Number(snapshot.registrationMonth.slice(5, 7));
  expect(snapshot.months.map((row: { month: number }) => row.month)).toEqual(Array.from({ length: 13 - firstMonth }, (_, i) => i + firstMonth));
  await expect(page.getByRole('cell', { name: 'In progress', exact: true })).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.screenshot({ path: testInfo.outputPath('reports.png'), fullPage: true });

  // A later server-side change must not change the already displayed report or its export.
  const csrf = (await (await page.request.get('/api/auth/csrf')).json()).token;
  const history = await (await page.request.get('/api/users/me/requirements')).json();
  const baseline = history.items.find((item: { isBaseline: boolean }) => item.isBaseline);
  const updated = await page.request.put(`/api/users/me/requirements/${baseline.id}`, {
    headers: { 'X-CSRF-TOKEN': csrf }, data: { startingFrom: baseline.startingFrom, requiredOfficePercentage: 80 }
  });
  expect(updated.ok()).toBe(true);
  let reportFetches = 0;
  page.on('request', request => { if (request.url().includes('/api/reports')) reportFetches++; });
  const downloadPromise = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export CSV' }).click();
  const download = await downloadPromise;
  const csv = await readFile((await download.path())!, 'utf8');
  expect(reportFetches).toBe(0);
  expect(csv).toContain(`"${username}","${snapshot.generatedAt}"`);
  for (const row of snapshot.months) {
    expect(csv).toContain(`"${row.eligibleWorkingDays}","50","${row.requiredOfficeDays}"`);
  }
  const refreshedPromise = page.waitForResponse(response => response.url().includes('/api/reports?') && response.ok());
  await page.getByRole('button', { name: 'Refresh report' }).click();
  const refreshed = await (await refreshedPromise).json();
  expect(refreshed.months.every((row: { requiredOfficePercentage: number }) => row.requiredOfficePercentage === 80)).toBe(true);
  await page.getByLabel('Year', { exact: true }).fill(String(snapshot.year - 1));
  await expect(page.getByRole('button', { name: 'Export CSV' })).toBeDisabled();
  await page.getByRole('button', { name: 'Generate report' }).click();
  await expect(page.getByText('No months in this range')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Export CSV' })).toBeDisabled();
});
