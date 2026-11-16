import { fireEvent, render, screen } from '@testing-library/svelte';
import { expect, it } from 'vitest';
import GuestApp from './GuestApp.svelte';
import { readGuestData, saveGuestData, type GuestData } from './storage';

it('Should_ApplyAndPersistAppearance_WhenGuestSavesProfile', async (): Promise<void> => {
  // Arrange
  const createdAt = '2026-06-01T00:00:00Z';
  const initial: GuestData = {
    version: 1, id: 'guest', createdAt, registrationMonth: '2026-06-01',
    countryCode: 'RO', countryName: 'Romania', timeZoneId: 'UTC', teamDay: null, appearance: 'System',
    requirements: [{ id: 'baseline', createdAt, startingFrom: '0001-01-01', requirementMode: 'Percentage', requirementValue: 50 }],
    attendance: [], plans: [], vacations: [], holidays: {}
  };
  saveGuestData(initial);
  const root = document.documentElement;
  root.dataset.appearance = 'Light';
  const { unmount } = render(GuestApp, { initial });
  await fireEvent.click(screen.getByRole('button', { name: 'Profile' }));
  const select = screen.getByLabelText('Appearance');

  const expectedTeamDay = 'monday';
  expect(screen.getByRole('option', { name: 'Monday' })).toBeVisible();

  // Act
  await fireEvent.change(screen.getByLabelText('Team day'), { target: { value: expectedTeamDay } });
  expect(readGuestData()?.teamDay).toBeNull();
  for (const choice of ['Dark', 'Light', 'System']) {
    await fireEvent.change(select, { target: { value: choice } });
    await fireEvent.click(screen.getByRole('button', { name: 'Save' }));

    // Assert
    expect(readGuestData()?.teamDay).toBe(expectedTeamDay);
    expect(root.dataset.guestAppearance).toBe(choice);
    expect(readGuestData()?.appearance).toBe(choice);
  }
  unmount();
  expect(root.dataset.guestAppearance).toBeUndefined();
  expect(root.dataset.appearance).toBe('Light');
  delete root.dataset.appearance;
  localStorage.clear();
});
