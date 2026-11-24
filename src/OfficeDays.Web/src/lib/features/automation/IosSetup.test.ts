import { fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import { afterEach, expect, it, vi } from 'vitest';
import { resetCsrf } from '$lib/api';
import IosSetup from './IosSetup.svelte';

const originalClipboard = Object.getOwnPropertyDescriptor(navigator, 'clipboard');
afterEach((): void => {
  if (originalClipboard) Object.defineProperty(navigator, 'clipboard', originalClipboard);
  else Reflect.deleteProperty(navigator, 'clipboard');
});

const shortcutUrl = 'https://www.icloud.com/shortcuts/example';
const deviceName = 'Work phone';
const testToken = 'sample-credential';

type FetchSetup = (path: string, options?: RequestInit) => Promise<Response>;

function setup(url: string | null, createStatus = 201): ReturnType<typeof vi.fn<FetchSetup>> {
  resetCsrf();
  const fetcher = vi.fn(async (path: string, options?: RequestInit): Promise<Response> => {
    if (path === '/api/automation/ios') return Response.json({ shortcutUrl: url });
    if (path === '/api/auth/csrf') return Response.json({ token: 'test-csrf' });
    if (path === '/api/tokens' && options?.method === 'POST')
      return Response.json(createStatus === 201 ? { token: testToken } : { detail: 'Could not create token.' }, { status: createStatus });
    throw new Error('Unexpected request');
  });
  vi.stubGlobal('fetch', fetcher);
  render(IosSetup);
  return fetcher;
}

async function start(): Promise<void> {
  await fireEvent.input(screen.getByLabelText('Device name'), { target: { value: deviceName } });
  const button = screen.getByRole('button', { name: 'Start' });
  await waitFor((): void => { expect(button).toBeEnabled(); });
  await fireEvent.click(button);
}

it('Should_DisableStart_WhenTemplateIsUnavailable', async (): Promise<void> => {
  // Arrange
  const fetcher = setup(null);

  // Act
  await fireEvent.input(screen.getByLabelText('Device name'), { target: { value: deviceName } });
  await screen.findByText(/Shortcut setup is unavailable/);

  // Assert
  expect(screen.getByRole('button', { name: 'Start' })).toBeDisabled();
  expect(fetcher).toHaveBeenCalledTimes(1);
});

it('Should_ShowOneTimeHeaderAndImportGuide_WhenDeviceTokenIsCreated', async (): Promise<void> => {
  // Arrange
  const fetcher = setup(shortcutUrl);
  const writeText = vi.fn().mockResolvedValue(undefined);
  Object.defineProperty(navigator, 'clipboard', { configurable: true, value: { writeText } });
  const expectedHeader = 'Bearer ' + testToken;
  expect(screen.queryByRole('heading', { name: 'Setup guide' })).toBeNull();
  expect(document.querySelectorAll('figure img[loading="eager"]')).toHaveLength(9);

  // Act
  await start();
  await fireEvent.click(await screen.findByRole('button', { name: 'Copy token' }));

  // Assert
  expect(fetcher).toHaveBeenCalledWith('/api/tokens', expect.objectContaining({
    body: JSON.stringify({ name: deviceName, purpose: 'ios-shortcuts' })
  }));
  expect(screen.getByLabelText('Authorization header')).toHaveValue(expectedHeader);
  expect(writeText).toHaveBeenCalledWith(expectedHeader);
  expect(await screen.findByRole('status')).toHaveTextContent('Authorization header copied.');
  expect(screen.getByRole('link', { name: 'Add to Shortcuts' })).toHaveAttribute('href', shortcutUrl);
  expect(screen.queryByRole('button', { name: 'Start' })).toBeNull();
  await fireEvent.click(screen.getByRole('button', { name: 'Next' }));
  expect(screen.getByRole('link', { name: 'Enlarge screenshot: Start the import' })).toBeVisible();
});

it('Should_SelectHeaderForManualCopy_WhenClipboardIsUnavailable', async (): Promise<void> => {
  // Arrange
  setup(shortcutUrl);
  Object.defineProperty(navigator, 'clipboard', { configurable: true, value: undefined });
  await start();

  // Act
  await fireEvent.click(await screen.findByRole('button', { name: 'Copy token' }));

  // Assert
  expect(await screen.findByRole('alert')).toHaveTextContent('Copy the selected text manually.');
  const input = screen.getByRole<HTMLInputElement>('textbox', { name: 'Authorization header' });
  expect(input).toHaveFocus();
  expect(input.value.slice(input.selectionStart ?? 0, input.selectionEnd ?? 0)).toBe('Bearer ' + testToken);
});

it('Should_KeepDeviceNameForRetry_WhenCreationFails', async (): Promise<void> => {
  // Arrange
  setup(shortcutUrl, 500);

  // Act
  await start();

  // Assert
  expect(await screen.findByRole('alert')).toHaveTextContent('Could not create token.');
  expect(screen.getByLabelText('Device name')).toHaveValue(deviceName);
  expect(screen.getByRole('button', { name: 'Start' })).toBeEnabled();
});
