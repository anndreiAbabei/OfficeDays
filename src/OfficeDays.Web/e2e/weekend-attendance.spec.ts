import { expect, test } from '@playwright/test';
import type { GuestData } from '../src/lib/features/guest/storage';

test('Should_ConfirmNonWorkingVisitsAndCreditOnlyWeekends_WhenGuestRecordsAttendance', async ({ page, context }): Promise<void> => {
  // Arrange
  const createdAt = '2026-06-01T12:00:00Z';
  const data: GuestData = {
    version: 1, id: 'weekend-guest', createdAt, registrationMonth: '2026-06-01',
    countryCode: 'RO', countryName: 'Romania', timeZoneId: 'UTC', teamDay: null,
    requirements: [{ id: 'baseline', createdAt, startingFrom: '0001-01-01', requirementMode: 'Percentage', requirementValue: 50 }],
    holidays: { '2026': [{ date: '2026-06-01', name: 'Holiday' }] },
    vacations: [{ id: 'vacation', createdAt, from: '2026-06-02', to: '2026-06-02' }],
    attendance: [], plans: []
  };
  await page.clock.setFixedTime(new Date('2026-06-15T12:00:00Z'));
  await page.addInitScript((guest): void => { localStorage.setItem('officedays.guest.v1', JSON.stringify(guest)); }, data);
  await page.goto('/guest');
  await expect(page.getByLabel('Monthly status')).toHaveAttribute('aria-busy', 'false');
  await context.setOffline(true);
  const officeMetric = page.getByRole('article').filter({ has: page.getByText('Office so far', { exact: true }) });
  const requiredMetric = page.getByRole('article').filter({ has: page.getByText('Required office', { exact: true }) });
  const remainingMetric = page.getByRole('article').filter({ has: page.getByText('Remaining', { exact: true }) });
  const expectedTarget = '10';
  const expectedOffice = '2';
  const expectedRemaining = '8';

  // Act: cancel the first Saturday entry.
  await page.getByLabel('Office attendance date').fill('2026-06-06');
  const cancelled = page.waitForEvent('dialog');
  const cancelledClick = page.getByRole('button', { name: 'Add day', exact: true }).click();
  const dialog = await cancelled;
  expect(dialog.message()).toContain('weekend');
  await dialog.dismiss();
  await cancelledClick;

  // Assert
  await expect(page.getByRole('button', { name: /Remove office day/ })).toHaveCount(0);
  await expect(officeMetric.locator('strong')).toHaveText('0');

  // Act: add both weekend days, retry Saturday, then add holiday and vacation visits.
  for (const date of ['2026-06-06', '2026-06-07', '2026-06-06', '2026-06-01', '2026-06-02']) {
    await page.getByLabel('Office attendance date').fill(date);
    const opened = page.waitForEvent('dialog');
    const confirmedClick = page.getByRole('button', { name: 'Add day', exact: true }).click();
    const confirmation = await opened;
    expect(confirmation.message()).toContain(date.endsWith('01') || date.endsWith('02') ? 'will not count' : 'It will count');
    await confirmation.accept();
    await confirmedClick;
    await expect(page.getByText('Office day recorded.', { exact: true })).toBeVisible();
    await expect(page.getByLabel('Monthly status')).toHaveAttribute('aria-busy', 'false');
  }

  // Assert
  await expect(page.getByRole('button', { name: /Remove office day/ })).toHaveCount(4);
  await expect(officeMetric.locator('strong')).toHaveText(expectedOffice);
  await expect(requiredMetric.locator('strong')).toHaveText(expectedTarget);
  await expect(remainingMetric.locator('strong')).toHaveText(expectedRemaining);
  await expect(page.getByRole('button', { name: /Jun 6, 2026.*Office attendance recorded/ })).toBeVisible();
  await page.getByRole('button', { name: 'Reports', exact: true }).click();
  const month = page.getByRole('row').filter({ has: page.getByRole('rowheader', { name: 'June' }) });
  await expect(month.getByRole('cell').nth(3)).toHaveText(expectedOffice);
  await expect(month.getByRole('cell').nth(2)).toHaveText(expectedTarget);
  expect(await page.evaluate((): boolean => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});
