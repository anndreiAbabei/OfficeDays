import { randomUUID } from 'node:crypto';
import { test, expect } from '@playwright/test';

test('Should_StayOnLogin_WhenProfileSaveCompletesAfterSignOut', async ({ page }): Promise<void> => {
  // Arrange
  const username = `session-${randomUUID()}`;
  const password = 'SessionRegression!123';
  const created = await page.request.post('/api/users', { data: { username, password, timeZoneId: 'UTC', countryCode: 'RO' } });
  expect(created.ok()).toBe(true);
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await page.getByRole('link', { name: 'Profile', exact: true }).click();

  let releaseResponse: () => void = (): void => {};
  let saveReachedServer: () => void = (): void => {};
  const release = new Promise<void>((resolve): void => { releaseResponse = resolve; });
  const saved = new Promise<void>((resolve): void => { saveReachedServer = resolve; });
  await page.route('**/api/users/me', async (route): Promise<void> => {
    const response = await route.fetch();
    saveReachedServer();
    await release;
    await route.fulfill({ response });
  });

  // Act: hold a successful profile response until the account has signed out.
  await page.getByLabel('Email address', { exact: true }).fill('late@example.com');
  await page.getByRole('button', { name: 'Save profile', exact: true }).click();
  await saved;
  await page.getByRole('button', { name: 'Sign out', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
  const delivered = page.waitForResponse((response): boolean => response.url().endsWith('/api/users/me'));
  releaseResponse();
  await delivered;

  // Assert: the late response cannot sign the user back in or redirect to the landing page.
  await page.getByLabel('Username', { exact: true }).fill(username);
  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('button', { name: 'Sign in', exact: true })).toBeVisible();
  const session = await page.request.get('/api/auth/me');
  const expectedStatus = 401;
  expect(session.status()).toBe(expectedStatus);
});
