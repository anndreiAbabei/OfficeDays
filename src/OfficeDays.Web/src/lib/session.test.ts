import { beforeEach, expect, it } from 'vitest';
import { get } from 'svelte/store';
import { createSessionUpdater, signedIn, signedOut, user } from './session';
import type { User } from './types';

const original: User = { appearance: 'System', id: 'user-1', username: 'Guest test', email: null, countryCode: 'RO', countryName: 'Romania', timeZoneId: 'UTC', isAdmin: false, requirementMode: 'Percentage', requirementValue: 50, teamDay: null };
beforeEach((): void => { signedOut(); });
it('Should_UpdateProfile_WhenTheOriginalSessionIsActive', (): void => {
  // Arrange
  signedIn(original);
  const update = createSessionUpdater();
  const expectedUser = { ...original, email: 'saved@example.com' };
  // Act
  update(expectedUser);
  // Assert
  expect(get(user)).toBe(expectedUser);
});
it('Should_StaySignedOut_WhenAProfileResponseArrivesAfterLogout', (): void => {
  // Arrange
  signedIn(original);
  const update = createSessionUpdater();
  signedOut();
  // Act
  update({ ...original, email: 'late@example.com' });
  // Assert
  expect(get(user)).toBeNull();
});
it.each(['user-1', 'user-2'])('Should_PreserveNewSession_WhenAnOldResponseArrivesFor %s', (id): void => {
  // Arrange
  signedIn(original);
  const update = createSessionUpdater();
  signedOut();
  const expectedUser = { ...original, id, email: 'new-session@example.com' };
  signedIn(expectedUser);
  // Act
  update({ ...original, email: 'late@example.com' });
  // Assert
  expect(get(user)).toBe(expectedUser);
});

it('Should_ApplyAccountAppearance_WhenSigningInAndSavingProfile', (): void => {
  // Arrange
  const expectedAppearance = 'Dark';
  signedIn({ ...original, appearance: 'Light' });
  expect(document.documentElement.dataset.appearance).toBe('Light');

  // Act
  createSessionUpdater()({ ...original, appearance: expectedAppearance });

  // Assert
  expect(document.documentElement.dataset.appearance).toBe(expectedAppearance);
  expect(document.cookie.split('; ')).toContain(`appearance=${expectedAppearance}`);
});

it.each(['Light', 'Dark', 'System'])('Should_RemoveAppearanceCookie_WhenSystemReplaces%s', (previous): void => {
  // Arrange
  document.cookie = `appearance=${previous}; Path=/`;

  // Act
  signedIn(original);

  // Assert
  expect(document.documentElement.dataset.appearance).toBe(original.appearance);
  expect(document.cookie.split('; ').some((cookie): boolean => cookie.startsWith('appearance='))).toBe(false);
});
