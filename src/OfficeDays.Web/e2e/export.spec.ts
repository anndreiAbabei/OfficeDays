import { test, expect } from '@playwright/test';
import { readFile } from 'node:fs/promises';

test('profile downloads personal JSON and lets the user retry a failure', async ({ page }, testInfo) => {
  const username = `export-${testInfo.project.name}-${Date.now()}`;
  expect((await page.request.post('/api/users', { data: {
    username, password: 'BrowserTestsOnly!123', timeZoneId: 'UTC', countryCode: 'RO'
  } })).ok()).toBe(true);
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await page.getByRole('link', { name: 'Profile', exact: true }).click();
  const button = page.getByRole('button', { name: 'Download my data (JSON)' });
  await page.route('**/api/users/me/export', route => route.fulfill({ status: 500, contentType: 'application/problem+json', body: '{"detail":"Export unavailable"}' }));
  await button.click();
  await expect(page.getByRole('alert')).toContainText('Export unavailable');
  await expect(button).toBeEnabled();
  await page.unroute('**/api/users/me/export');
  const downloadPromise = page.waitForEvent('download');
  await button.click();
  const download = await downloadPromise;
  expect(download.suggestedFilename()).toBe('officedays-data.json');
  await download.saveAs(testInfo.outputPath('personal-data.json'));
  const raw = await readFile((await download.path())!, 'utf8');
  const data = JSON.parse(raw);
  expect(data.formatVersion).toBe(1);
  expect(data.profile.username).toBe(username);
  expect(data.application.version).toBeTruthy();
  expect(data.holidayCalendar.code).toBe('RO');
  expect(data.requirementHistory[0].startingFrom).toBe('0001-01-01');
  expect(raw).not.toMatch(/password|tokenHash/i);
  await expect(page.getByRole('status')).toContainText('Download started.');
  await expect(page.getByRole('alert')).toHaveCount(0);
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.screenshot({ path: testInfo.outputPath('export.png'), fullPage: true });
});
