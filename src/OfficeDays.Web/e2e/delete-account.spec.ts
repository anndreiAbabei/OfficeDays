import { test, expect } from '@playwright/test';

test('Should_CancelRetryAndSignOut_WhenDeletingAccount', async ({ page }, testInfo): Promise<void> => {
  // Arrange
  const username = `delete-${Date.now()}`;
  const password = 'DeleteAccountTest!123';
  await page.request.post('/api/users', { data: { username, password, timeZoneId: 'UTC', countryCode: 'RO' } });
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.locator('.calendar')).toBeVisible();
  await page.goto('/profile');
  const trigger = page.getByRole('button', { name: 'Delete your account', exact: true });
  await trigger.click();
  const dialog = page.getByRole('dialog');
  await expect(dialog).toBeVisible();
  for (const colorScheme of ['light', 'dark'] as const) {
    await page.emulateMedia({ colorScheme });
    expect(await page.evaluate((): boolean => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
    await page.screenshot({ path: testInfo.outputPath(`delete-account-${colorScheme}.png`), fullPage: true });
  }
  await expect(page.getByLabel('Current password')).toBeFocused();
  await page.keyboard.press('Escape');
  await expect(dialog).not.toBeVisible();
  await expect(trigger).toBeFocused();
  await trigger.click();
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(trigger).toBeFocused();
  await trigger.click();
  await page.getByLabel('Current password').fill('wrong-password');
  await page.getByRole('button', { name: 'Permanently delete account' }).click();
  await expect(dialog.getByRole('alert')).toContainText('incorrect');
  await expect(page.getByLabel('Current password')).toHaveValue('');
  // Act
  await page.getByLabel('Current password').fill(password);
  await page.getByRole('button', { name: 'Permanently delete account' }).click();
  // Assert
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('status')).toContainText('have been deleted');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('incorrect');
});
