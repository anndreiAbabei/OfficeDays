 import { test, expect } from '@playwright/test';

test('Should_ShowRetryGuidance_WhenLoginIsRateLimited', async ({ page }): Promise<void> => {
  // Arrange
  const expectedMessage = 'Too many requests. Please try again in 30 seconds.';
  await page.route('**/api/auth/login', async (route): Promise<void> => {
    await route.fulfill({ status: 429, headers: { 'Retry-After': '30' },
      contentType: 'application/problem+json', body: JSON.stringify({ title: 'Too many requests' }) });
  });
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill('admin');
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');

  // Act
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();

  // Assert
  await expect(page.getByRole('alert')).toHaveText(expectedMessage);
  await expect(page.getByRole('button', { name: 'Sign in', exact: true })).toBeEnabled();
});
