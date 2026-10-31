import { test, expect, type Page } from '@playwright/test';

async function setupGuest(page: Page): Promise<void> {
  await page.clock.setFixedTime(new Date('2026-06-15T12:00:00Z'));
  await page.goto('/');
  await page.getByRole('link', { name: 'Continue as guest' }).click();
  await expect(page.getByLabel('Country / jurisdiction')).toHaveValue('');
  await page.getByLabel('Country / jurisdiction').selectOption('RO');
  await page.getByLabel('Timezone', { exact: true }).fill('UTC');
  await page.getByRole('button', { name: 'Start tracking' }).click();
  await expect(page.getByText('Saved in this browser', { exact: true })).toBeVisible();
  await expect(page.getByLabel('Monthly status')).toHaveAttribute('aria-busy', 'false');
}

test('Should_TrackReportAndExportWithoutRequests_WhenGuestGoesOffline', async ({ page, context }, testInfo): Promise<void> => {
  // Arrange
  await setupGuest(page);
  const requests: string[] = [];
  page.on('request', (request): void => { if (request.url().includes('/api/')) requests.push(request.url()); });
  await page.screenshot({ path: testInfo.outputPath('guest-dashboard.png'), fullPage: true });
  await context.setOffline(true);
  const date = '2026-06-15';
  // Act
  await page.getByLabel('Office attendance date').fill(date);
  await page.getByRole('button', { name: 'Add day', exact: true }).click();
  await expect(page.getByText('Office day recorded.')).toBeVisible();
  await page.getByLabel('From', { exact: true }).fill(date);
  await page.getByLabel('To', { exact: true }).fill(date);
  await page.getByRole('button', { name: 'Add', exact: true }).click();
  await expect(page.getByText('Vacation added.')).toBeVisible();
  await page.getByRole('button', { name: /Toggle plan/ }).first().click();
  await expect(page.getByRole('button', { name: /Planned.*Toggle plan/i }).first()).toBeVisible();
  await page.getByRole('button', { name: 'Reports', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Monthly breakdown' })).toBeVisible();
  await page.getByLabel('Year', { exact: true }).fill('2030');
  await expect(page.getByText('Holidays for 2030 have not been downloaded.', { exact: false })).toBeVisible();
  await page.getByRole('button', { name: 'Generate report', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Monthly breakdown' })).toBeVisible();
  await page.getByRole('button', { name: 'Profile', exact: true }).click();
  const downloadPromise = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download guest data' }).click();
  const download = await downloadPromise;
  // Assert
  const expectedFilename = 'officedays-guest-data.json';
  expect(download.suggestedFilename()).toBe(expectedFilename);
  expect(requests).toEqual([]);
  expect(await page.evaluate((): boolean => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});

test('Should_ReturnToGuestDashboard_WhenSetupAlreadyExists', async ({ page }): Promise<void> => {
  // Arrange
  await setupGuest(page);
  // Act
  await page.goto('/');
  // Assert
  await expect(page).toHaveURL(/\/guest$/);
  await expect(page.getByText('Saved in this browser', { exact: true })).toBeVisible();
});

test('Should_AskBeforeOpeningGuestData_WhenSessionCannotBeChecked', async ({ page }): Promise<void> => {
  // Arrange
  await setupGuest(page);
  await page.route('**/api/auth/me', (route) => route.abort());
  // Act
  await page.goto('/');
  // Assert
  await expect(page.getByRole('button', { name: 'Retry connection' })).toBeVisible();
  await page.getByRole('button', { name: 'Open saved guest data' }).click();
  await expect(page.getByText('Saved in this browser', { exact: true })).toBeVisible();
});

test('Should_KeepGuestDataUntouched_WhenFullAccountSignsIn', async ({ page }): Promise<void> => {
  // Arrange
  await setupGuest(page);
  const before = await page.evaluate((): string | null => localStorage.getItem('officedays.guest.v1'));
  // Act
  await page.getByRole('link', { name: 'Sign in', exact: true }).click();
  await page.getByLabel('Username', { exact: true }).fill('admin');
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  // Assert
  await expect(page.getByRole('link', { name: 'API tokens', exact: true })).toBeVisible();
  await page.goto('/guest');
  await expect(page).toHaveURL(/\/$/);
  expect(await page.evaluate((): string | null => localStorage.getItem('officedays.guest.v1'))).toBe(before);
});

test('Should_DownloadMissingHolidayYearOnlyOnRequest_WhenGuestNavigates', async ({ page }): Promise<void> => {
  // Arrange
  await setupGuest(page);
  let downloads = 0;
  await page.route('**/api/bank-holidays/RO/2030', async (route): Promise<void> => {
    downloads++;
    await route.fulfill({ json: [{ date: '2030-06-03', name: 'Test holiday' }] });
  });
  // Act
  await page.getByLabel('Year', { exact: true }).fill('2030');
  await page.getByLabel('Year', { exact: true }).press('Tab');
  await expect(page.getByText('Holidays for 2030 have not been downloaded.', { exact: false })).toBeVisible();
  const expectedInitialDownloads = 0;
  expect(downloads).toBe(expectedInitialDownloads);
  await page.getByRole('button', { name: 'Get holidays', exact: true }).click();
  // Assert
  await expect(page.getByRole('button', { name: 'Get holidays', exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: /Test holiday/ })).toBeVisible();
  const expectedDownloads = 1;
  expect(downloads).toBe(expectedDownloads);
  await page.reload();
  await page.getByLabel('Year', { exact: true }).fill('2030');
  await page.getByLabel('Year', { exact: true }).press('Tab');
  await expect(page.getByRole('button', { name: /Test holiday/ })).toBeVisible();
  expect(downloads).toBe(expectedDownloads);
});
