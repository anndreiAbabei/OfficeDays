import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import Tokens from './Tokens.svelte';
import { resetCsrf } from '$lib/api';

function setup(fail = false, status = 'Active') {
  resetCsrf();
  const fetcher = vi.fn(async (path: string, options?: RequestInit) => {
    if (path.endsWith('csrf')) return Response.json({ token: 'csrf' });
    if (options?.method === 'PATCH') {
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

it('saves the trimmed display name inline, including for revoked tokens', async () => {
  const fetcher = setup(false, 'Revoked');
  await fireEvent.click(await screen.findByRole('button', { name: 'Rename Phone' }));
  const input = screen.getByLabelText('New token name');
  expect(input).toHaveValue('Phone');
  await fireEvent.input(input, { target: { value: '  Work phone  ' } });
  await fireEvent.submit(input.closest('form')!);
  await screen.findByRole('button', { name: 'Rename Work phone' });
  const [, options] = fetcher.mock.calls.find(([, options]) => options?.method === 'PATCH')!;
  expect(JSON.parse(String(options?.body))).toEqual({ name: 'Work phone' });
  expect(screen.getByText('Revoked')).toBeInTheDocument();
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
