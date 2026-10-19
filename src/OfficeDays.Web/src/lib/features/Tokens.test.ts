import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import Tokens from './Tokens.svelte';
import { resetCsrf } from '$lib/api';

function setup(fail = false, status = 'Active') {
  resetCsrf();
  const fetcher = vi.fn(async (path: string, options?: RequestInit) => {
    if (path.endsWith('csrf')) return Response.json({ token: 'csrf' });
    if (options?.method === 'PATCH' || options?.method === 'DELETE') {
      return fail
        ? Response.json({ detail: 'Could not save name.' }, { status: 500 })
        : new Response(null, { status: 204 });
    }
    return Response.json([{ id: 'token-1', name: 'Phone', createdAt: '2026-10-17T09:00:00Z', lastUsedAt: null, status }]);
  });
  vi.stubGlobal('fetch', fetcher);
  render(Tokens);
  return fetcher;
}

it('saves the trimmed display name inline', async () => {
  const fetcher = setup();
  await fireEvent.click(await screen.findByRole('button', { name: 'Rename Phone' }));
  const input = screen.getByLabelText('New token name');
  expect(input).toHaveValue('Phone');
  await fireEvent.input(input, { target: { value: '  Work phone  ' } });
  await fireEvent.submit(input.closest('form')!);
  await screen.findByRole('button', { name: 'Rename Work phone' });
  const [, options] = fetcher.mock.calls.find(([, options]) => options?.method === 'PATCH')!;
  expect(JSON.parse(String(options?.body))).toEqual({ name: 'Work phone' });
  expect(screen.getByText('Active')).toBeInTheDocument();
  expect(screen.getByRole('status')).toHaveTextContent('Token renamed.');
});

it('cancels edits without sending an update', async () => {
  const fetcher = setup();
  await fireEvent.click(await screen.findByRole('button', { name: 'Rename Phone' }));
  await fireEvent.input(screen.getByLabelText('New token name'), { target: { value: 'Changed' } });
  await fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
  expect(screen.getByRole('button', { name: 'Rename Phone' })).toBeInTheDocument();
  expect(fetcher.mock.calls.some(([, options]) => options?.method === 'PATCH')).toBe(false);
});

it('prevents blank names and retains input after a failed save', async () => {
  setup(true);
  await fireEvent.click(await screen.findByRole('button', { name: 'Rename Phone' }));
  const input = screen.getByLabelText('New token name');
  expect(input).toHaveAttribute('maxlength', '100');
  await fireEvent.input(input, { target: { value: '   ' } });
  expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
  await fireEvent.input(input, { target: { value: 'Work phone' } });
  await fireEvent.submit(input.closest('form')!);
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Could not save name.'));
  expect(input).toHaveValue('Work phone');
  expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
});

it('hides previously revoked tokens when loading the list', async () => {
  setup(false, 'Revoked');
  await screen.findByText('No active API tokens.');
  expect(screen.queryByRole('button', { name: 'Rename Phone' })).not.toBeInTheDocument();
  expect(screen.queryByText('Phone')).not.toBeInTheDocument();
});

it('removes a token from the list after revocation and keeps active tokens', async () => {
  resetCsrf();
  let revoked = false;
  const fetcher = vi.fn(async (path: string, options?: RequestInit) => {
    if (path.endsWith('csrf')) return Response.json({ token: 'csrf' });
    if (options?.method === 'DELETE') {
      revoked = true;
      return new Response(null, { status: 204 });
    }
    return Response.json([
      { id: 'phone', name: 'Phone', createdAt: '2026-10-17T09:00:00Z', lastUsedAt: null, status: revoked ? 'Revoked' : 'Active' },
      { id: 'laptop', name: 'Laptop', createdAt: '2026-10-17T09:00:00Z', lastUsedAt: null, status: 'Active' }
    ]);
  });
  vi.stubGlobal('fetch', fetcher);
  vi.stubGlobal('confirm', vi.fn().mockReturnValue(true));
  render(Tokens);
  const rename = await screen.findByRole('button', { name: 'Rename Phone' });
  await fireEvent.click(rename.parentElement!.querySelector('button.danger')!);
  await waitFor(() => expect(screen.queryByText('Phone')).not.toBeInTheDocument());
  expect(screen.getByRole('button', { name: 'Rename Laptop' })).toBeInTheDocument();
  expect(screen.getByRole('status')).toHaveTextContent('Token revoked.');
  expect(fetcher.mock.calls.some(([path, options]) => path === '/api/tokens/phone' && options?.method === 'DELETE')).toBe(true);
});

it('shows revoked tokens on demand and hides them again', async () => {
  setup(false, 'Revoked');
  await screen.findByText('No active API tokens.');
  const toggle = screen.getByRole('checkbox', { name: 'Show revoked tokens' });
  expect(toggle).not.toBeChecked();
  await fireEvent.click(toggle);
  expect(screen.getByText('Phone')).toBeInTheDocument();
  expect(screen.getByText('Revoked')).toBeInTheDocument();
  expect(screen.queryByRole('button', { name: 'Revoke' })).not.toBeInTheDocument();
  await fireEvent.click(toggle);
  expect(screen.queryByText('Phone')).not.toBeInTheDocument();
  expect(screen.getByText('No active API tokens.')).toBeInTheDocument();
});

it('deletes a revoked token only after confirmation', async () => {
  const fetcher = setup(false, 'Revoked');
  const confirm = vi.fn().mockReturnValue(false);
  vi.stubGlobal('confirm', confirm);
  await screen.findByText('No active API tokens.');
  await fireEvent.click(screen.getByRole('checkbox', { name: 'Show revoked tokens' }));
  await fireEvent.click(screen.getByRole('button', { name: 'Delete' }));
  expect(confirm).toHaveBeenCalledWith(expect.stringContaining('cannot be undone'));
  expect(fetcher.mock.calls.some(([, options]) => options?.method === 'DELETE')).toBe(false);
  expect(screen.getByText('Phone')).toBeInTheDocument();
  confirm.mockReturnValue(true);
  await fireEvent.click(screen.getByRole('button', { name: 'Delete' }));
  await screen.findByText('No API tokens yet.');
  expect(screen.getByRole('status')).toHaveTextContent('Token permanently deleted.');
  expect(fetcher.mock.calls.some(([path, options]) => path === '/api/tokens/token-1/permanent' && options?.method === 'DELETE')).toBe(true);
});

it('keeps a revoked token visible when permanent deletion fails', async () => {
  setup(true, 'Revoked');
  vi.stubGlobal('confirm', vi.fn().mockReturnValue(true));
  await screen.findByText('No active API tokens.');
  await fireEvent.click(screen.getByRole('checkbox', { name: 'Show revoked tokens' }));
  await fireEvent.click(screen.getByRole('button', { name: 'Delete' }));
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Could not save name.'));
  expect(screen.getByText('Phone')).toBeInTheDocument();
  expect(screen.getByRole('button', { name: 'Delete' })).toBeEnabled();
});

it('does not offer permanent deletion for active tokens', async () => {
  setup();
  await screen.findByRole('button', { name: 'Revoke' });
  await fireEvent.click(screen.getByRole('checkbox', { name: 'Show revoked tokens' }));
  expect(screen.queryByRole('button', { name: 'Delete' })).not.toBeInTheDocument();
});
