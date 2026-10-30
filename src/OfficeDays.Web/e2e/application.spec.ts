import { test, expect, type Page } from '@playwright/test';

async function login(page: Page, username: string, password = 'BrowserTestsOnly!123') {
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  await expect(page.getByLabel('Monthly status')).toHaveAttribute('aria-busy', 'false');
}
async function noOverflow(page: Page) {
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
}

test('Should_SupportAccountWorkflow_WhenUsingTheRealApi', async ({ page }, testInfo) => {
  const errors: string[] = []; page.on('pageerror', error => errors.push(error.message));
  const username = `user-${testInfo.project.name}-${Date.now()}`;
  await page.goto('/#register');
  await expect(page).toHaveURL(/\/register$/);
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByLabel('Confirm password').fill('DifferentPassword!123');
  await page.getByLabel('Timezone').fill('UTC');
  await page.getByRole('combobox', { name: 'Holiday calendar' }).selectOption('RO');
  await page.getByRole('button', { name: 'Create account', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('Passwords do not match.');
  await page.getByLabel('Confirm password').fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Create account', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Account created. Sign in to continue.');
  await login(page, username);
  await expect(page.getByRole('link', { name: 'Administration' })).toHaveCount(0);
  await expect(page.locator('.app-footer')).not.toContainText('Loading version');
  await noOverflow(page);
  const date = new Date().toISOString().slice(0, 10);
  await page.getByLabel('Office attendance date').fill(date);
  await page.getByRole('button', { name: 'Add day', exact: true }).click();
  await expect(page.getByText('Office day recorded.')).toBeVisible();
  await expect(page.getByRole('button', { name: /Remove office day/ })).toHaveCount(1);
  await page.getByLabel('From', { exact: true }).fill(date);
  await page.getByLabel('To', { exact: true }).fill(date);
  await page.getByRole('button', { name: 'Add', exact: true }).click();
  await expect(page.getByText('Vacation added.')).toBeVisible();
  await expect(page.getByRole('button', { name: /Remove vacation/ })).toHaveCount(1);
  const columns = await page.locator('.columns > section').evaluateAll(elements => elements.map(el => ({ x: el.getBoundingClientRect().x, y: el.getBoundingClientRect().y })));
  if (testInfo.project.name.startsWith('desktop')) expect(columns[0].y).toBe(columns[1].y);
  else expect(columns[0].x).toBe(columns[1].x);
  await page.screenshot({ path: testInfo.outputPath('dashboard.png'), fullPage: true });
  await page.getByRole('button', { name: /Remove vacation/ }).click();
  await expect(page.getByText('Vacation removed.')).toBeVisible();
  await page.getByRole('button', { name: /Remove office day/ }).click();
  await expect(page.getByText('Office day removed.')).toBeVisible();
  await page.getByRole('link', { name: 'Profile', exact: true }).click();
  await page.getByLabel('Email address', { exact: true }).fill('person@example.com');
  await page.getByRole('button', { name: 'Save profile' }).click();
  await expect(page.getByText('Profile saved.')).toBeVisible();
  await page.getByRole('button', { name: 'Edit Initial requirement', exact: true }).click();
  await page.getByLabel('Required office days (%)').fill('60');
  await page.getByRole('button', { name: 'Save requirement', exact: true }).click();
  await expect(page.getByText('Attendance requirement saved.')).toBeVisible();
  await page.reload();
  await expect(page.getByText('Initial requirement · 60%')).toBeVisible();
  await noOverflow(page);
  await page.screenshot({ path: testInfo.outputPath('requirements.png'), fullPage: true });
  await page.getByRole('link', { name: 'API tokens', exact: true }).click();
  await page.getByLabel('Token name').fill('Browser test');
  await page.getByRole('button', { name: 'Create token' }).click();
  await expect(page.locator('.token-reveal code')).toContainText('odt_');
  page.once('dialog', dialog => dialog.accept());
  await page.getByRole('button', { name: 'Revoke' }).click();
  await expect(page.getByText('Token revoked.', { exact: true })).toBeVisible();
  await page.getByRole('checkbox', { name: 'Show revoked tokens' }).check();
  await expect(page.getByText('Revoked', { exact: true })).toBeVisible();
  await noOverflow(page);
  await page.goto('/admin');
  await expect(page.getByText('Administrator access is required.')).toBeVisible();
  await page.getByRole('button', { name: 'Sign out' }).click();
  await expect(page.getByRole('button', { name: 'Sign in', exact: true })).toBeVisible();
  await page.goto('/profile');
  await expect(page).toHaveURL(/\/login$/);
  expect(errors).toEqual([]);
});

test('admin manages jurisdictions and holidays, protects unsaved changes', async ({ page }, testInfo) => {
  await login(page, 'Admin');
  await page.getByRole('link', { name: 'Administration' }).click();
  const section = page.locator('section').filter({ has: page.getByRole('heading', { name: 'Holiday jurisdictions' }) });
  const code = { 'desktop-chromium': 'TC', 'iphone-webkit': 'TW', 'android-chromium': 'US' }[testInfo.project.name]!;
  await section.getByLabel('Code', { exact: true }).fill(code);
  await section.getByLabel('Name', { exact: true }).fill('Test calendar');
  await section.getByRole('button', { name: 'Add jurisdiction' }).click();
  await expect(section.getByText('Holiday jurisdiction saved.')).toBeVisible();
  const row = section.getByRole('listitem').filter({ hasText: code });
  page.once('dialog', dialog => dialog.accept('Renamed calendar'));
  await row.getByRole('button', { name: 'Rename' }).click();
  await expect(row.getByText('Renamed calendar')).toBeVisible();
  await page.getByRole('combobox', { name: 'Jurisdiction', exact: true }).selectOption(code);
  const holidays = page.locator('section').filter({ has: page.getByRole('heading', { name: 'Bank holidays', exact: true }) });
  await expect(holidays.getByRole('button', { name: 'Add / update' })).toBeEnabled();
  const year = await holidays.getByLabel('Year', { exact: true }).inputValue();
  await holidays.getByLabel('Date', { exact: true }).fill(`${year}-01-01`);
  await holidays.getByLabel('Holiday name', { exact: true }).fill('New Year');
  await holidays.getByRole('button', { name: 'Add / update' }).click();
  page.once('dialog', dialog => dialog.dismiss());
  await page.getByRole('link', { name: 'Dashboard', exact: true }).click();
  await expect(page).toHaveURL(/\/admin$/);
  await holidays.getByRole('button', { name: 'Save year' }).click();
  await expect(holidays.getByText('Bank holidays saved.')).toBeVisible();
  await noOverflow(page);
  await page.screenshot({ path: testInfo.outputPath('admin.png'), fullPage: true });
  await page.reload();
  await page.getByRole('combobox', { name: 'Jurisdiction', exact: true }).selectOption(code);
  await expect(holidays.getByText('New Year', { exact: true })).toBeVisible();
  await holidays.getByRole('button', { name: 'Remove', exact: true }).click();
  await holidays.getByRole('button', { name: 'Save year' }).click();
  await expect(holidays.getByText('Bank holidays saved.')).toBeVisible();
  page.once('dialog', dialog => dialog.accept());
  await section.getByRole('listitem').filter({ hasText: code }).getByRole('button', { name: 'Remove', exact: true }).click();
  await expect(section.getByText('Holiday jurisdiction removed.')).toBeVisible();
});

test('summary renders before delayed lists and homepage requests exclude other features', async ({ page }, testInfo) => {
  await login(page, 'Admin');
  const requests: string[] = [];
  page.on('request', request => { if (request.url().includes('/api/')) requests.push(request.url()); });
  let release!: () => void;
  const gate = new Promise<void>(resolve => release = resolve);
  await page.route(/\/api\/(attendance|vacations)\?/, async route => { await gate; await route.continue(); });
  const start = Date.now();
  await page.reload();
  await expect(page.getByLabel('Monthly status')).toHaveAttribute('aria-busy', 'false');
  const elapsed = Date.now() - start;
  await expect(page.getByText('Loading office days…')).toBeVisible();
  await expect(page.getByText('Loading vacations…')).toBeVisible();
  release();
  expect(requests.some(url => /\/api\/(tokens|bank-holidays|holiday-jurisdictions)/.test(url))).toBe(false);
  await testInfo.attach('summary-timing', { body: JSON.stringify({ localReloadSummaryMs: elapsed, note: 'Local test only; not a mobile-network benchmark.' }), contentType: 'application/json' });
});

test('expired sessions return to login and dark narrow layouts remain usable', async ({ page }) => {
  await login(page, 'Admin');
  await page.emulateMedia({ colorScheme: 'dark' });
  await page.setViewportSize({ width: 320, height: 740 });
  await noOverflow(page);
  await page.context().clearCookies();
  await page.getByRole('button', { name: 'Next month' }).click();
  await expect(page.getByRole('button', { name: 'Sign in', exact: true })).toBeVisible();
  await noOverflow(page);
});

test('cold authenticated homepage meets the initial script budget on simulated mobile network', async ({ page, context, browserName }, testInfo) => {
  test.skip(browserName !== 'chromium', 'Network/CPU emulation uses Chromium CDP.');
  await login(page, 'Admin');
  const coldPage = await context.newPage();
  const client = await context.newCDPSession(coldPage);
  await client.send('Network.enable');
  await client.send('Network.clearBrowserCache');
  await client.send('Network.setCacheDisabled', { cacheDisabled: true });
  await client.send('Network.emulateNetworkConditions', {
    offline: false, latency: 150, downloadThroughput: 200_000, uploadThroughput: 93_750, connectionType: 'cellular4g'
  });
  await client.send('Emulation.setCPUThrottlingRate', { rate: 4 });
  const start = Date.now();
  await coldPage.goto('/');
  await expect(coldPage.getByLabel('Monthly status')).toHaveAttribute('aria-busy', 'false', { timeout: 15_000 });
  const summaryMs = Date.now() - start;
  const resources = await coldPage.evaluate(() => performance.getEntriesByType('resource').map(entry => {
    const resource = entry as PerformanceResourceTiming;
    return { path: new URL(resource.name).pathname, bytes: resource.encodedBodySize, decodedBytes: resource.decodedBodySize };
  }));
  const scripts = resources.filter(resource => resource.path.endsWith('.js'));
  const scriptBytes = scripts.reduce((total, resource) => total + resource.bytes, 0);
  expect(scriptBytes).toBeGreaterThan(0);
  expect(scriptBytes).toBeLessThan(100 * 1024);
  await testInfo.attach('cold-homepage-performance', {
    body: JSON.stringify({ summaryMs, scriptBytes, latencyMs: 150, downloadBytesPerSecond: 200000, cpuSlowdown: 4, resources }, null, 2),
    contentType: 'application/json'
  });
  await coldPage.close();
});
