import { expect, test } from '@playwright/test';

test('Should_RememberAppearanceAcrossRoutes_WhenProfileIsSaved', async ({ page, context }): Promise<void> => {
  // Arrange
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill('Admin');
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.locator('.calendar')).toBeVisible();
  const body = page.locator('body');
  const light = 'rgb(243, 245, 241)';
  const dark = 'rgb(15, 19, 16)';

  for (const choice of ['Dark', 'Light', 'System'] as const) {
    // Act
    await page.emulateMedia({ colorScheme: choice === 'Dark' ? 'light' : 'dark' });
    await page.goto('/profile');
    await page.getByLabel('Appearance').selectOption(choice);
    await page.getByRole('button', { name: 'Save profile' }).click();

    // Assert
    await expect(page.getByText('Profile saved.', { exact: true })).toBeVisible();
    expect((await context.cookies()).find(cookie => cookie.name === 'appearance')?.value).toBe(choice);
    await page.goto('/');
    await expect(page.locator('.calendar')).toBeVisible();
    await expect(body).toHaveCSS('background-color', choice === 'Light' ? light : dark);
    if (choice === 'System') {
      await page.emulateMedia({ colorScheme: 'light' });
      await expect(body).toHaveCSS('background-color', light);
    }
    await page.goto('/profile');
    await expect(page.getByLabel('Appearance')).toHaveValue(choice);
  }
});
