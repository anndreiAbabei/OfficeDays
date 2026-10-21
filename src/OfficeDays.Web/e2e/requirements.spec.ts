import { test, expect } from '@playwright/test';

test('requirements can be scheduled, edited and deleted without changing the baseline', async ({ page }, testInfo) => {
  const username = `req-${testInfo.project.name}-${Date.now()}`;
  await page.request.post('/api/users', { data: { username, password: 'BrowserTestsOnly!123', timeZoneId: 'UTC', countryCode: 'RO' } });
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await page.getByRole('link', { name: 'Profile', exact: true }).click();
  await expect(page.getByText('Initial requirement · 50%')).toBeVisible();
  await page.getByRole('button', { name: 'Add requirement', exact: true }).click();
  await page.getByLabel('Effective from').fill('2099-01');
  await page.getByLabel('Required office days (%)').fill('60');
  await page.getByRole('button', { name: 'Save requirement', exact: true }).click();
  await expect(page.getByText('January 2099 · 60%')).toBeVisible();
  await page.getByRole('button', { name: 'Edit January 2099', exact: true }).click();
  await page.getByLabel('Effective from').fill('2099-02');
  await page.getByLabel('Required office days (%)').fill('70');
  await page.getByRole('button', { name: 'Save requirement', exact: true }).click();
  await expect(page.getByText('February 2099 · 70%')).toBeVisible();
  await page.reload();
  await expect(page.getByText('February 2099 · 70%')).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.screenshot({ path: testInfo.outputPath('requirement-history.png'), fullPage: true });
  await page.getByRole('button', { name: 'Delete February 2099', exact: true }).click();
  await page.getByRole('button', { name: 'Confirm remove', exact: true }).click();
  await expect(page.getByText('No upcoming changes.')).toBeVisible();
  await expect(page.getByText('Initial requirement · 50%')).toBeVisible();
});

test('Should_PreserveWeeklyQuota_WhenRegisteringAndChangingRequirementMode', async ({ page }, testInfo) => {
  // Arrange
  const username = `weekly-${testInfo.project.name}-${Date.now()}`;
  await page.goto('/register');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByLabel('Confirm password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByLabel('Timezone', { exact: true }).fill('UTC');
  await page.getByLabel('Holiday calendar').selectOption('RO');

  // Act
  await page.getByLabel('Requirement type').selectOption('DaysPerWeek');
  await expect(page.getByText(/Averaged over the month/)).toBeVisible();
  await page.getByLabel('Office days per week').fill('3');
  await page.getByRole('button', { name: 'Create account', exact: true }).click();
  await expect(page).toHaveURL(/login/);
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await page.getByRole('link', { name: 'Profile', exact: true }).click();

  // Assert
  await expect(page.getByText('Initial requirement · 3 days per week')).toBeVisible();
  await page.getByRole('button', { name: 'Edit Initial requirement', exact: true }).click();
  await expect(page.getByLabel('Requirement type')).toHaveValue('DaysPerWeek');
  await expect(page.getByLabel('Office days per week')).toHaveValue('3');
  await page.screenshot({ path: testInfo.outputPath('weekly-requirement.png'), fullPage: true });
  await page.getByLabel('Requirement type').selectOption('Percentage');
  await page.getByLabel('Required office days (%)').fill('65');
  await page.getByRole('button', { name: 'Save requirement', exact: true }).click();
  await expect(page.getByText('Initial requirement · 65%')).toBeVisible();
  await page.reload();
  await expect(page.getByText('Initial requirement · 65%')).toBeVisible();
  expect(await page.evaluate((): boolean => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});
