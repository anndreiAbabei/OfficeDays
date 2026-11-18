import { render, screen } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import VersionInfo from './VersionInfo.svelte';

it.each([true, false])('Should_DescribeUpdate_WhenAvailabilityIs%s', async (available) => {
  // Arrange
  const release = { version: '2.4.0+local', latestVersion: '2.5.0', newVersionAvailable: available,
    author: 'Andrei', gitHubRepo: 'https://github.com/example/OfficeDays' };
  const expectedTooltip = '2.5.0 version is available';
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(Response.json(release)));

  // Act
  render(VersionInfo);

  // Assert
  expect(await screen.findByText(/v2\.4\.0\+local/)).toBeVisible();
  expect(screen.getByRole('link', { name: release.author })).toHaveAttribute('href', release.gitHubRepo);
  if (available) {
    expect(screen.getByTitle(expectedTooltip)).toHaveTextContent('v2.4.0+local');
    expect(screen.getByRole('strong')).toHaveAttribute('title', expectedTooltip);
  } else {
    expect(screen.queryByRole('strong')).toBeNull();
    expect(screen.queryByTitle(expectedTooltip)).toBeNull();
  }
});

it('Should_ShowFallback_WhenVersionRequestFails', async () => {
  // Arrange
  vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')));

  // Act
  render(VersionInfo);

  // Assert
  expect(screen.getByText(/Loading version/)).toBeVisible();
  expect(await screen.findByText(/Version unavailable/)).toBeVisible();
  expect(screen.queryByRole('strong')).toBeNull();
});
