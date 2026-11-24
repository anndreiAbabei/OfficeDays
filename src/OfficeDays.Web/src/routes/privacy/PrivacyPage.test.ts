import { render, screen } from '@testing-library/svelte';
import { afterEach, expect, it } from 'vitest';
import { configuration } from '$lib/features/privacy/configuration';
import PrivacyPage from './+page.svelte';

afterEach((): void => configuration.set({ status: 'loading' }));

it('Should_ShowOperatorAndRetention_WhenPrivacyNoticeIsPublished', (): void => {
  // Arrange
  const contactEmail = 'privacy@example.org';
  const details = 'Retention for this instance.';
  configuration.set({ status: 'ready', persistentLoginSeconds: 86400,
    privacyNotice: { operatorName: 'Example operator', contactEmail, details } });

  // Act
  render(PrivacyPage);

  // Assert
  expect(screen.getByText(/Example operator operates this instance/)).toBeVisible();
  expect(screen.getByRole('link', { name: contactEmail })).toHaveAttribute('href', 'mailto:' + contactEmail);
  expect(screen.getByText(details)).toBeVisible();
  expect(screen.getByText(/Inactivity alone does not close it/)).toBeVisible();
});

it('Should_DisplayPlaceholderAsDraft_WhenMailboxIsUnconfigured', (): void => {
  // Arrange
  const contactEmail = 'contact@officedays.test.com';
  configuration.set({ status: 'ready', persistentLoginSeconds: 86400,
    privacyNotice: { operatorName: 'OfficeDays Team', contactEmail, details: 'Draft for review.' } });

  // Act
  render(PrivacyPage);

  // Assert
  expect(screen.getByText(/Draft privacy contact/)).toHaveTextContent(contactEmail);
  expect(screen.queryByRole('link', { name: contactEmail })).toBeNull();
});
