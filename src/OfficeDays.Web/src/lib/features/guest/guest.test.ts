import scenarios from '../../../../../../tests/Fixtures/attendance-calculations.json';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createGuestSource } from './dataSource';
import { guestStorageKey, readGuestData, saveGuestData, type GuestData } from './storage';
import { percentage, planning, report, status, todayIn } from './calculations';
import { guestExport } from './export';

const now = new Date('2026-06-15T12:00:00Z');
function fixture(): GuestData {
  return { version: 1, id: 'guest', createdAt: now.toISOString(), registrationMonth: '2026-06-01', countryCode: 'RO', countryName: 'Romania', timeZoneId: 'UTC', teamDay: null,
    requirements: [{ id: 'baseline', createdAt: now.toISOString(), startingFrom: '0001-01-01', requirementMode: 'Percentage', requirementValue: 50 }],
    attendance: [], plans: [], vacations: [], holidays: { '2026': [], '2027': [] } };
}
beforeEach((): void => { localStorage.clear(); vi.restoreAllMocks(); });
describe('guest data', (): void => {
  it('Should_SaveAndReload_WhenSetupIsComplete', (): void => {
    // Arrange
    const expected = fixture();
    // Act
    saveGuestData(expected);
    // Assert
    expect(readGuestData()).toEqual(expected);
  });
  it.each(['{', '{"version":2}', '{"version":1}'])('Should_PreserveData_WhenStorageIsInvalid (%s)', (raw): void => {
    // Arrange
    localStorage.setItem(guestStorageKey, raw);
    // Act / Assert
    expect((): GuestData | null => readGuestData()).toThrow(/preserved/);
    expect(localStorage.getItem(guestStorageKey)).toBe(raw);
  });
  it('Should_KeepPreviousData_WhenSavingFails', async (): Promise<void> => {
    // Arrange
    saveGuestData(fixture());
    const expected = localStorage.getItem(guestStorageKey);
    const source = createGuestSource(localStorage, (): Date => now);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation((): never => { throw new Error('quota'); });
    // Act / Assert
    await expect(source.mutate('/api/attendance/2026-06-12', 'PUT')).rejects.toThrow('Changes were not saved');
    expect(localStorage.getItem(guestStorageKey)).toBe(expected);
  });
  it('Should_RecordOnlyOnceWithoutNetwork_WhenAttendanceIsRepeated', async (): Promise<void> => {
    // Arrange
    saveGuestData(fixture());
    const fetchSpy = vi.spyOn(globalThis, 'fetch');
    const source = createGuestSource(localStorage, (): Date => now);
    // Act
    await source.mutate('/api/attendance/2026-06-12', 'PUT');
    await source.mutate('/api/attendance/2026-06-12', 'PUT');
    await source.request('/api/status?year=2026&month=6');
    await source.request('/api/reports?year=2026');
    // Assert
    const expectedCount = 1;
    expect(readGuestData()?.attendance).toHaveLength(expectedCount);
    expect(fetchSpy).not.toHaveBeenCalled();
    await expect(source.request('/api/tokens')).rejects.toThrow('full account');
    expect(fetchSpy).not.toHaveBeenCalled();
  });
  it.each(['2026-05-29', '2026-06-16', '2026-02-30'])('Should_RejectAttendance_WhenDateIsOutsideTrackingBounds (%s)', async (date): Promise<void> => {
    // Arrange
    saveGuestData(fixture());
    const source = createGuestSource(localStorage, (): Date => now);
    // Act / Assert
    await expect(source.mutate(`/api/attendance/${date}`, 'PUT')).rejects.toThrow();
    expect(readGuestData()?.attendance).toEqual([]);
  });
  it('Should_KeepEarlierMonthsEditable_WhenTimeAdvances', async (): Promise<void> => {
    // Arrange
    saveGuestData(fixture());
    const source = createGuestSource(localStorage, (): Date => new Date('2026-07-02T12:00:00Z'));
    // Act
    await source.mutate('/api/attendance/2026-06-12', 'PUT');
    // Assert
    const expectedDate = '2026-06-12';
    expect(readGuestData()?.attendance[0].date).toBe(expectedDate);
  });
  it('Should_RejectPastAndIneligiblePlans_WhenChangingPlans', async (): Promise<void> => {
    // Arrange
    saveGuestData(fixture());
    const source = createGuestSource(localStorage, (): Date => now);
    // Act / Assert
    await expect(source.mutate('/api/planning/2026-06-12', 'DELETE')).rejects.toThrow('Past plans');
    await expect(source.mutate('/api/planning/2026-06-20', 'PUT')).rejects.toThrow('eligible');
    await source.mutate('/api/planning/2026-06-16', 'PUT');
    expect(readGuestData()?.attendance).toEqual([]);
    const expectedCount = 1;
    expect(readGuestData()?.plans).toHaveLength(expectedCount);
  });
  it('Should_ManageVacationsAndRequirements_WhenWorkingLocally', async (): Promise<void> => {
    // Arrange
    saveGuestData(fixture());
    const source = createGuestSource(localStorage, (): Date => now);
    // Act
    await source.mutate('/api/vacations', 'POST', { from: '2026-06-16', to: '2026-06-17' });
    const id = readGuestData()?.vacations[0].id;
    await source.mutate(`/api/vacations/${id}`, 'PUT', { from: '2026-06-16', to: '2026-06-18' });
    await source.mutate('/api/users/me/requirements', 'POST', { startingFrom: '2026-07-01', requirementMode: 'DaysPerWeek', requirementValue: 2 });
    // Assert
    const expectedTo = '2026-06-18';
    expect(readGuestData()?.vacations[0].to).toBe(expectedTo);
    await expect(source.mutate('/api/users/me/requirements/baseline', 'DELETE')).rejects.toThrow('initial requirement');
    await expect(source.mutate('/api/users/me/requirements', 'POST', { startingFrom: '2026-07-01', requirementMode: 'Percentage', requirementValue: 75 })).rejects.toThrow('already exists');
    await source.mutate(`/api/vacations/${id}`, 'DELETE');
    expect(readGuestData()?.vacations).toEqual([]);
  });
});
describe('guest calculations', (): void => {
  it('Should_CountEligibleDaysAndRoundOnce_WhenHolidaysAndVacationsOverlap', (): void => {
    // Arrange
    const data = fixture();
    data.requirements[0].requirementMode = 'DaysPerWeek'; data.requirements[0].requirementValue = 2;
    data.holidays['2026'] = [{ date: '2026-06-01', name: 'Holiday' }];
    data.vacations = [{ id: 'v', createdAt: now.toISOString(), from: '2026-06-01', to: '2026-06-02' }];
    data.attendance = ['2026-06-01', '2026-06-03', '2026-06-06'].map((date): GuestData['attendance'][number] => ({ id: date, date, isManual: true, createdAt: now.toISOString() }));
    // Act
    const result = status(data, { year: 2026, month: 6 }, now);
    // Assert
    const expected = { eligibleWorkingDays: 20, requiredOfficeDays: 8, officeDays: 1, progressPercentage: 12.5 };
    expect(result).toMatchObject(expected);
  });
  it('Should_MatchServerRounding_WhenPercentageIsMidpoint', (): void => {
    // Arrange
    const expected = 6.2;
    // Act / Assert
    expect(percentage(1, 16)).toBe(expected);
  });
  it('Should_RespectRegistrationAndFutureMonths_WhenGeneratingReports', (): void => {
    // Arrange
    const data = fixture();
    // Act
    const result = report(data, 2026, 1, 12, now);
    // Assert
    const expectedMonths = 7;
    expect(result.months).toHaveLength(expectedMonths);
    const expectedCurrent = 'inProgress'; const expectedFuture = 'notStarted';
    expect(result.months[0].result).toBe(expectedCurrent);
    expect(result.months[1].result).toBe(expectedFuture);
    expect(result.months[1].officeDays).toBeNull();
    const expectedCompleted = 0;
    expect(result.summary.completedMonths).toBe(expectedCompleted);
  });
  it('Should_UseConfiguredTimezone_WhenUtcDateDiffers', (): void => {
    // Arrange
    const data = fixture(); data.timeZoneId = 'Pacific/Auckland';
    const expectedDate = '2027-01-01';
    // Act / Assert
    expect(todayIn(data, new Date('2026-12-31T12:00:00Z'))).toBe(expectedDate);
  });
  it('Should_IncludeLeapDay_WhenCalculatingFebruary', (): void => {
    // Arrange
    const data = fixture(); const expectedDays = 29;
    // Act / Assert
    expect(planning(data, { year: 2028, month: 2 }, now).days).toHaveLength(expectedDays);
  });
  it('Should_ExportGuestFormatAndAllPersonalData_WhenDownloadingBackup', (): void => {
    // Arrange
    const data = fixture(); const expectedFormat = 'officedays-guest-data';
    // Act
    const result = guestExport(data, now);
    // Assert
    expect(result).toMatchObject({ format: expectedFormat, attendance: [], plans: [], vacations: [], preferences: { teamDay: null }, holidayCalendar: { downloadedYears: [2026, 2027] } });
  });
});

it.each(scenarios)('Should_MatchServerScenario_WhenCalculatingMonth $year-$month At $value $mode', (scenario): void => {
  // Arrange
  const data = fixture();
  data.requirements[0].requirementMode = scenario.mode === 'DaysPerWeek' ? 'DaysPerWeek' : 'Percentage';
  data.requirements[0].requirementValue = scenario.value;
  data.holidays[String(scenario.year)] = scenario.holidays.map((date): { date: string; name: string } => ({ date, name: 'Holiday' }));
  data.vacations = scenario.vacations.map((v, index): GuestData['vacations'][number] => ({ ...v, id: String(index), createdAt: now.toISOString() }));
  data.attendance = scenario.attendance.map((date): GuestData['attendance'][number] => ({ date, id: date, isManual: true, createdAt: now.toISOString() }));
  const expected = { eligibleWorkingDays: scenario.eligible, requiredOfficeDays: scenario.required, officeDays: scenario.recorded, progressPercentage: scenario.progress };
  // Act
  const actual = status(data, scenario, new Date('2029-01-01T12:00:00Z'));
  // Assert
  expect(actual).toMatchObject(expected);
});
