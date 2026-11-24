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
    const iosLink = screen.getByRole('link', { name: 'iOS/iPadOS' });
    const macLink = screen.getByRole('link', { name: 'macOS' });
    expect(iosLink).toHaveAttribute('href', '/automation/ios');
    expect(iosLink.closest('li')).toHaveTextContent('iOS/iPadOS — Shortcuts');
    expect(macLink).toHaveAttribute('href', '/automation/macos');
    expect(macLink.closest('li')).toHaveTextContent('macOS — Shortcuts (coming soon)');
  } finally {
    browser.mockRestore();
  }
});
