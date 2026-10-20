import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import PersonalDataDownload from './PersonalDataDownload.svelte';

it('downloads the export and shows loading and completion feedback', async () => {
  let resolve!: (response: Response) => void;
  const fetcher = vi.fn(() => new Promise<Response>(done => { resolve = done; }));
  vi.stubGlobal('fetch', fetcher);
  const create = vi.fn(() => 'blob:export');
  const revoke = vi.fn();
  vi.stubGlobal('URL', Object.assign(class extends URL {}, { createObjectURL: create, revokeObjectURL: revoke }));
  const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {});
  render(PersonalDataDownload);
  await fireEvent.click(screen.getByRole('button', { name: 'Download my data (JSON)' }));
  expect(screen.getByRole('button', { name: 'Preparing download…' })).toBeDisabled();
  expect(fetcher).toHaveBeenCalledWith('/api/users/me/export', expect.objectContaining({ credentials: 'same-origin', cache: 'no-store' }));
  resolve(new Response('{"formatVersion":1}', { headers: { 'Content-Type': 'application/json' } }));
  await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('Download started.'));
  expect(create).toHaveBeenCalledWith(expect.any(Blob));
  expect(click).toHaveBeenCalledOnce();
  expect(document.querySelector('a[download]')).toBeNull();
  await waitFor(() => expect(revoke).toHaveBeenCalledWith('blob:export'), { timeout: 2000 });
  click.mockRestore();
});

it.each([
  [401, 'Please sign in again.'],
  [500, 'Request failed (500)']
])('shows an export failure (%s) and allows retry without downloading an error', async (status, message) => {
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('{}', { status })));
  const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {});
  render(PersonalDataDownload);
  await fireEvent.click(screen.getByRole('button', { name: 'Download my data (JSON)' }));
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent(message));
  expect(screen.getByRole('button', { name: 'Download my data (JSON)' })).toBeEnabled();
  expect(click).not.toHaveBeenCalled();
  click.mockRestore();
});
