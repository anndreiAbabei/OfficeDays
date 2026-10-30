import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { afterEach, beforeEach, expect, it, vi } from 'vitest';
import GuestSetup from './GuestSetup.svelte';
import { request } from '$lib/api';
import { readGuestData } from './storage';
vi.mock('$lib/api', () => ({ request: vi.fn(), errorMessage: (error: unknown): string => error instanceof Error ? error.message : 'Error' }));
beforeEach((): void => { localStorage.clear(); vi.clearAllMocks(); vi.useFakeTimers({ toFake: ['Date'] }); vi.setSystemTime(new Date('2026-06-15T12:00:00Z')); });
afterEach((): void => { vi.useRealTimers(); });
it('Should_SaveSetupWithTwoHolidayYears_WhenGuestStartsTracking', async (): Promise<void> => {
  // Arrange
  vi.mocked(request).mockResolvedValueOnce([{ code: 'RO', name: 'Romania' }]).mockResolvedValue([]);
  const oncomplete = vi.fn();
  render(GuestSetup, { oncomplete });
  const country = await screen.findByRole('combobox', { name: 'Country / jurisdiction' });
  const expectedDefault = 50;
  expect(country).toHaveValue('');
  expect(screen.getByLabelText('Required office days (%)')).toHaveValue(expectedDefault);
  // Act
  await fireEvent.change(country, { target: { value: 'RO' } });
  await fireEvent.input(screen.getByLabelText('Timezone'), { target: { value: 'UTC' } });
  await fireEvent.click(screen.getByRole('button', { name: 'Start tracking' }));
  // Assert
  await waitFor((): void => { expect(oncomplete).toHaveBeenCalledOnce(); });
  const expectedYears = ['2026', '2027'];
  expect(Object.keys(readGuestData()?.holidays ?? {})).toEqual(expectedYears);
});
it('Should_KeepSetupIncomplete_WhenHolidayDownloadFails', async (): Promise<void> => {
  // Arrange
  vi.mocked(request).mockResolvedValueOnce([{ code: 'RO', name: 'Romania' }]).mockRejectedValue(new Error('Offline'));
  const oncomplete = vi.fn();
  render(GuestSetup, { oncomplete });
  // Act
  await fireEvent.change(await screen.findByRole('combobox', { name: 'Country / jurisdiction' }), { target: { value: 'RO' } });
  await fireEvent.click(screen.getByRole('button', { name: 'Start tracking' }));
  // Assert
  const expectedError = 'Offline';
  expect(await screen.findByRole('alert')).toHaveTextContent(expectedError);
  expect(readGuestData()).toBeNull();
  expect(oncomplete).not.toHaveBeenCalled();
});
