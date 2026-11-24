import { render, screen, within } from '@testing-library/svelte';
import { expect, it } from 'vitest';
import CookieNotice from './CookieNotice.svelte';

it('Should_ConnectFooterAndCloseButtonsToThePanel_WhenRenderingCookieInformation', (): void => {
  render(CookieNotice, { persistentLoginSeconds: 1728000 });
  const trigger = screen.getByRole('button', { name: 'Cookies and browser storage' });
  const panel = screen.getByRole('dialog', { hidden: true });
  const close = within(panel).getByRole('button', { name: 'Close cookie information', hidden: true });

  expect(trigger).toHaveAttribute('popovertarget', panel.id);
  expect(panel).toHaveAttribute('popover', 'auto');
  expect(close).toHaveAttribute('popovertarget', panel.id);
  expect(close).toHaveAttribute('popovertargetaction', 'hide');
  expect(panel).toHaveTextContent('20 days');
  for (const name of ['OfficeDays.Session', 'OfficeDays.Antiforgery', 'appearance', 'Clearing site data erases your guest history']) {
    expect(panel).toHaveTextContent(name);
  }
});
