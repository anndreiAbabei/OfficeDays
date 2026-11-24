import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { beforeEach, expect, it, vi } from 'vitest';
import AttendanceList from './AttendanceList.svelte';
import { useDataSource } from './dataSource';
import type { PlanningDay } from '$lib/types';

vi.mock('./dataSource', (): object => ({ useDataSource: vi.fn() }));
const request = vi.fn();
const mutate = vi.fn();
const onchange = vi.fn();
const confirm = vi.fn();
const selectedDate = '2026-09-05';

beforeEach((): void => {
  vi.resetAllMocks();
  vi.mocked(useDataSource).mockReturnValue({ request, mutate });
  vi.stubGlobal('confirm', confirm);
});

async function addDay(day: Partial<PlanningDay>): Promise<void> {
  request.mockImplementation(async (path: string): Promise<unknown> => path.startsWith('/api/planning')
    ? { days: [{ date: selectedDate, isEligible: false, isVacation: false, holidays: [], ...day }] }
    : []);
  render(AttendanceList, { period: { year: 2026, month: 10 }, timeZone: 'UTC', onchange });
  await screen.findByText('No office days recorded for this month.');
  await fireEvent.input(screen.getByLabelText('Office attendance date'), { target: { value: selectedDate } });
  await fireEvent.click(screen.getByRole('button', { name: 'Add day' }));
}

it.each([
  [{}, 'weekend', 'It will count toward your office target'],
  [{ holidays: ['Holiday'] }, 'bank holiday or vacation', 'will not count'],
  [{ isVacation: true }, 'bank holiday or vacation', 'will not count']
])('Should_AskBeforeRecordingNonWorkingAttendance_WhenCalendarHas%s', async (day, reason, explanation): Promise<void> => {
  // Arrange
  confirm.mockReturnValue(true);
  // Act
  await addDay(day);
  // Assert
  await waitFor((): void => { expect(onchange).toHaveBeenCalledOnce(); });
  expect(confirm).toHaveBeenCalledWith(expect.stringContaining(reason));
  expect(confirm).toHaveBeenCalledWith(expect.stringContaining(explanation));
  expect(request).toHaveBeenCalledWith('/api/planning?year=2026&month=9');
  expect(mutate).toHaveBeenCalledWith(`/api/attendance/${selectedDate}`, 'PUT');
});

it('Should_LeaveAttendanceUntouched_WhenConfirmationIsCancelled', async (): Promise<void> => {
  // Arrange
  confirm.mockReturnValue(false);
  // Act
  await addDay({});
  // Assert
  await waitFor((): void => { expect(confirm).toHaveBeenCalledOnce(); });
  expect(mutate).not.toHaveBeenCalled();
  expect(onchange).not.toHaveBeenCalled();
  expect(screen.getByRole('button', { name: 'Add day' })).toBeEnabled();
});

it('Should_SaveWithoutConfirmation_WhenDateIsEligible', async (): Promise<void> => {
  // Arrange / Act
  await addDay({ isEligible: true });
  // Assert
  await waitFor((): void => { expect(onchange).toHaveBeenCalledOnce(); });
  expect(confirm).not.toHaveBeenCalled();
});

it('Should_AllowRetryWithoutRefreshingTotals_WhenRecordingFails', async (): Promise<void> => {
  // Arrange
  confirm.mockReturnValue(true);
  mutate.mockRejectedValue(new Error('Could not save.'));
  // Act
  await addDay({});
  // Assert
  expect(await screen.findByRole('alert')).toHaveTextContent('Could not save.');
  expect(onchange).not.toHaveBeenCalled();
  expect(screen.getByRole('button', { name: 'Add day' })).toBeEnabled();
});

it('Should_LeaveAttendanceUntouched_WhenCalendarLookupFails', async (): Promise<void> => {
  // Arrange
  request.mockImplementation(async (path: string): Promise<unknown> => {
    if (path.startsWith('/api/planning')) throw new Error('Calendar unavailable.');
    return [];
  });
  render(AttendanceList, { period: { year: 2026, month: 9 }, timeZone: 'UTC', onchange });
  await screen.findByText('No office days recorded for this month.');
  // Act
  await fireEvent.click(screen.getByRole('button', { name: 'Add day' }));
  // Assert
  expect(await screen.findByRole('alert')).toHaveTextContent('Calendar unavailable.');
  expect(mutate).not.toHaveBeenCalled();
  expect(confirm).not.toHaveBeenCalled();
});

it('Should_DisableRepeatedSubmissions_WhenAttendanceWriteIsPending', async (): Promise<void> => {
  // Arrange
  let complete: (() => void) | undefined;
  mutate.mockImplementation((): Promise<void> => new Promise<void>((resolve): void => { complete = resolve; }));
  // Act
  await addDay({ isEligible: true });
  // Assert
  await waitFor((): void => { expect(mutate).toHaveBeenCalledOnce(); });
  expect(screen.getByRole('button', { name: 'Add day' })).toBeDisabled();
  expect(onchange).not.toHaveBeenCalled();
  complete?.();
  await waitFor((): void => { expect(onchange).toHaveBeenCalledOnce(); });
  await waitFor((): void => { expect(screen.getByRole('button', { name: 'Add day' })).toBeEnabled(); });
});
