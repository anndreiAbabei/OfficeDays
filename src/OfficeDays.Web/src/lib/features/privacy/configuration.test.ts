import { get } from 'svelte/store';
import { expect, it, vi } from 'vitest';
import { configuration, loadConfiguration } from './configuration';

const published = { operatorName: 'Office', contactEmail: 'privacy@example.org', details: 'Instance notice' };
const persistentLoginSeconds = 86400;

it.each([null, published])('Should_LoadNoticeVisibility_WhenServerReturns%s', async (privacyNotice): Promise<void> => {
  // Arrange
  const expectedState = { status: 'ready', privacyNotice, persistentLoginSeconds };
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(Response.json({ privacyNotice, persistentLoginSeconds })));

  // Act
  await loadConfiguration();

  // Assert
  expect(get(configuration)).toEqual(expectedState);
});

it('Should_RecoverPublicSettings_WhenRetriedAfterConnectionFailure', async (): Promise<void> => {
  // Arrange
  const privacyNotice = published;
  const expectedState = { status: 'ready', privacyNotice, persistentLoginSeconds };
  vi.stubGlobal('fetch', vi.fn().mockRejectedValueOnce(new TypeError('offline'))
    .mockResolvedValueOnce(Response.json({ privacyNotice, persistentLoginSeconds })));

  // Act
  await loadConfiguration();
  expect(get(configuration).status).toBe('error');
  const retry = loadConfiguration();
  expect(get(configuration).status).toBe('loading');
  await retry;

  // Assert
  expect(get(configuration)).toEqual(expectedState);
});
