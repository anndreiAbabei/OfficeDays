import { fireEvent, render, screen } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import AutomationOverview from './AutomationOverview.svelte';

it('Should_OfferPhoneSetupFromDesktop_WhenOtherDeviceIsOpened', async (): Promise<void> => {
  // Arrange
  const browser = vi.spyOn(navigator, 'userAgent', 'get').mockReturnValue('Windows NT 10.0');
  try {
    render(AutomationOverview);
    expect(screen.getByRole('link', { name: 'Windows automation' })).toHaveAttribute('href', '/automation/windows');

    // Act
    await fireEvent.click(screen.getByText('Other device'));

    // Assert
    expect(screen.getByRole('link', { name: 'iOS/iPadOS — Shortcuts' })).toHaveAttribute('href', '/automation/ios');
    expect(screen.getByRole('link', { name: /macOS — Shortcuts/ })).toHaveAttribute('href', '/automation/macos');
  } finally {
    browser.mockRestore();
  }
});
