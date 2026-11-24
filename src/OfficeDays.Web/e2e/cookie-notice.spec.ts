import { expect, test } from '@playwright/test';

test('Should_ExplainBrowserStorage_WhenOpeningTheNoticeBeforeSignIn', async ({ page, context }): Promise<void> => {
  // Arrange
  const expectedNotice = 'Public notice for the test instance.';
  await page.route('**/api/configuration', (route): Promise<void> => route.fulfill({ json: {
    persistentLoginSeconds: 86400,
    privacyNotice: { operatorName: 'Example operator', contactEmail: 'privacy@example.org', details: expectedNotice }
  } }));
  await page.goto('/login');
  const footer = page.getByRole('contentinfo');
  const trigger = footer.getByRole('button', { name: 'Cookies and browser storage' });
  const panel = page.getByRole('dialog', { name: 'Cookies used by Office Days' });
  const heading = footer.getByRole('heading', { name: 'Cookies used by Office Days' });
  const remember = page.getByRole('checkbox', { name: 'Keep me signed in on this device (1 day)' });
  await expect(remember).not.toBeChecked();
  const initialCookies = await context.cookies();
  await expect(heading).toBeHidden();

  // Act
  await trigger.focus();
  const footerHeight = await footer.evaluate((element): number => element.offsetHeight);
  await trigger.press('Enter');

  // Assert
  await expect(heading).toBeVisible();
  await expect(panel).toHaveCSS('position', 'fixed');
  expect(await footer.evaluate((element): number => element.offsetHeight)).toBe(footerHeight);
  const close = panel.getByRole('button', { name: 'Close cookie information' });
  await expect(close).toBeFocused();
  for (const name of ['OfficeDays.Session', 'OfficeDays.Antiforgery', 'appearance']) {
    await expect(footer.getByText(name, { exact: true })).toBeVisible();
  }
  await expect(footer.getByText(/Clearing site data erases your guest history/)).toBeVisible();
  expect(await context.cookies()).toEqual(initialCookies);
  await page.keyboard.press('Escape');
  await expect(panel).toBeHidden();
  await expect(trigger).toBeFocused();
  await trigger.click();
  await close.click();
  await expect(panel).toBeHidden();
  await trigger.click();
  await page.mouse.click(2, 2);
  await expect(panel).toBeHidden();
  await footer.getByRole('link', { name: 'Privacy notice' }).click();
  await expect(page.getByText(expectedNotice, { exact: true })).toBeVisible();
  await expect(page.getByRole('link', { name: 'privacy@example.org' })).toHaveAttribute('href', 'mailto:privacy@example.org');
  await page.goto('/register');
  await expect(page.locator('form').getByRole('link', { name: /privacy notice/i })).toHaveAttribute('href', '/privacy');
});

test('Should_HideNotices_WhenInstanceUsesDefaultConfiguration', async ({ page }): Promise<void> => {
  // Arrange
  await page.goto('/register');

  // Act
  await expect(page.getByRole('button', { name: 'Create account' })).toBeEnabled();

  // Assert
  await expect(page.getByRole('link', { name: /privacy notice/i })).toHaveCount(0);
  await expect(page.getByText('Cookies and browser storage')).toHaveCount(0);
});
