import { expect, test } from '@playwright/test';
import type { GuestData } from '../src/lib/features/guest/storage';

test('Should_ExplainAndRefreshMonthlyCalculation_WhenSignedInUserChangesVacation', async ({ page, request }, testInfo): Promise<void> => {
  // Arrange: each browser project owns its calendar and user.
  const code = testInfo.project.name === 'desktop-chromium' ? 'GB-SD' : testInfo.project.name === 'iphone-webkit' ? 'GB-SI' : 'GB-SA';
  const username = `summary-${testInfo.project.name}-${Date.now()}`;
  const login = await request.post('/api/auth/login', { data: { username: 'Admin', password: 'BrowserTestsOnly!123' } });
  expect(login.ok()).toBe(true);
  const csrf = await (await request.get('/api/auth/csrf')).json() as { token: string };
  const headers = { 'X-CSRF-TOKEN': csrf.token };
  const calendar = await request.post('/api/holiday-jurisdictions', { data: { code, name: 'Summary test calendar' }, headers });
  expect(calendar.ok()).toBe(true);
  const holidays = await request.put(`/api/bank-holidays/${code}/2099`, { data: [
    { date: '2099-10-01', name: 'Summary weekday holiday' }, { date: '2099-10-03', name: 'Summary weekend holiday' }
  ], headers });
  expect(holidays.ok()).toBe(true);
  const registration = await request.post('/api/users', { data: { username, password: 'BrowserTestsOnly!123', countryCode: code, timeZoneId: 'UTC' } });
  expect(registration.ok()).toBe(true);
  await page.goto('/login');
  await page.getByLabel('Username', { exact: true }).fill(username);
  await page.getByLabel('Password', { exact: true }).fill('BrowserTestsOnly!123');
  await page.getByRole('button', { name: 'Sign in', exact: true }).click();
  const picker = page.getByLabel('Selected month');
  await picker.getByLabel('Year').fill('2099');
  await picker.getByLabel('Year').press('Tab');
  await picker.getByRole('combobox').selectOption('10');
  await page.getByLabel('From', { exact: true }).fill('2099-10-01');
  await page.getByLabel('To', { exact: true }).fill('2099-10-02');
  await page.getByRole('button', { name: 'Add', exact: true }).click();
  await expect(page.getByText('Vacation added.')).toBeVisible();
  await page.getByRole('button', { name: /Oct 30, 2099.*Toggle plan/ }).click();
  await expect(page.getByRole('region', { name: 'Plan office days' })).toHaveAttribute('aria-busy', 'false');
  const toggle = page.getByRole('button', { name: 'How this is calculated' });
  await expect(toggle).toBeEnabled();
  await expect(toggle).toHaveAttribute('aria-expanded', 'false');

  // Act
  await toggle.click();

  // Assert
  const calendarCalculation = page.getByRole('region', { name: 'Calendar calculation' });
  const target = page.getByRole('region', { name: 'Office target calculation' });
  await expect(calendarCalculation).toContainText('20 eligible working days');
  await expect(calendarCalculation.getByRole('list', { name: 'Bank holiday dates' })).toContainText('Summary weekend holiday');
  await expect(target).toContainText('Without holidays or vacation, your target would be 11 office days');
  await expect(target.locator('dl > div').filter({ has: page.getByText('Planned days this month', { exact: true }) })).toContainText('1 day');

  // Act: keep the explanation open while a vacation invalidates the planned day.
  await page.getByLabel('From', { exact: true }).fill('2099-10-30');
  await page.getByLabel('To', { exact: true }).fill('2099-10-30');
  await page.getByRole('button', { name: 'Add', exact: true }).click();

  // Assert
  await expect(calendarCalculation).toContainText('19 eligible working days');
  await expect(target.locator('dl > div').filter({ has: page.getByText('Planned days this month', { exact: true }) })).toContainText('0 days');
  for (const colorScheme of ['light', 'dark'] as const) {
    await page.emulateMedia({ colorScheme });
    expect(await page.evaluate((): boolean => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  }
  await page.locator('.progress-card').screenshot({ path: testInfo.outputPath('attendance-summary.png') });
  await page.getByRole('button', { name: 'Hide calculation' }).click();
  await expect(calendarCalculation).toHaveCount(0);
});

test('Should_UpdateOutstandingDaysOffline_WhenGuestRecordsToday', async ({ page, context }, testInfo): Promise<void> => {
  // Arrange
  const createdAt = '2026-10-01T12:00:00Z';
  const data: GuestData = {
    version: 1, id: 'summary-guest', createdAt, registrationMonth: '2026-10-01',
    countryCode: 'RO', countryName: 'Romania', timeZoneId: 'UTC', teamDay: null,
    requirements: [{ id: 'baseline', createdAt, startingFrom: '0001-01-01', requirementMode: 'Percentage', requirementValue: 50 }],
    holidays: { '2026': [{ date: '2026-10-01', name: 'Weekday holiday' }, { date: '2026-10-03', name: 'Weekend holiday' }] },
    vacations: [{ id: 'vacation', createdAt, from: '2026-10-01', to: '2026-10-02' }],
    attendance: [5, 6, 7, 8, 9, 12, 13, 14].map((day): GuestData['attendance'][number] => ({ id: String(day), createdAt, date: `2026-10-${String(day).padStart(2, '0')}`, isManual: true })),
    plans: ['2026-10-05', '2026-10-06', '2026-10-30'].map((date): GuestData['plans'][number] => ({ id: date, createdAt, date }))
  };
  await page.clock.setFixedTime(new Date('2026-10-30T12:00:00Z'));
  await page.addInitScript((guest): void => { localStorage.setItem('officedays.guest.v1', JSON.stringify(guest)); }, data);
  await page.goto('/guest');
  await expect(page.getByRole('button', { name: 'How this is calculated' })).toBeEnabled();
  await context.setOffline(true);
  const requests: string[] = [];
  page.on('request', (request): void => { if (request.url().includes('/api/')) requests.push(request.url()); });

  // Act
  await page.getByRole('button', { name: 'How this is calculated' }).click();
  const target = page.getByRole('region', { name: 'Office target calculation' });

  // Assert
  await expect(target).toContainText('2 days still required − 1 day available');
  await expect(target).toContainText('1 day that cannot be covered this month');

  // Act
  await page.getByLabel('Office attendance date').fill('2026-10-30');
  await page.getByRole('button', { name: 'Add day', exact: true }).click();

  // Assert
  await expect(target).toContainText('1 day still required − 0 days available');
  await expect(target).toContainText('1 day that cannot be covered this month');
  await expect(target.locator('dl > div').filter({ has: page.getByText('Office days recorded', { exact: true }) })).toContainText('9 days');
  expect(requests).toEqual([]);
  expect(await page.evaluate((): boolean => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.locator('.progress-card').screenshot({ path: testInfo.outputPath('guest-attendance-summary.png') });
});
