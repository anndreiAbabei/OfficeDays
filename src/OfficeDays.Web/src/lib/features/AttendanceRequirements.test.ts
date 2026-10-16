import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import AttendanceRequirements from './AttendanceRequirements.svelte';
import { resetCsrf } from '$lib/api';

const initial = [
  { id: 'baseline', startingFrom: '0001-01-01', requiredOfficePercentage: 50, isBaseline: true },
  { id: 'past', startingFrom: '2026-01-01', requiredOfficePercentage: 60, isBaseline: false },
  { id: 'later', startingFrom: '2027-04-01', requiredOfficePercentage: 80, isBaseline: false },
  { id: 'next', startingFrom: '2027-01-01', requiredOfficePercentage: 70, isBaseline: false }];
const path = '/api/users/me/requirements';
function mockApi() {
  resetCsrf(); let rows = structuredClone(initial);
  const fetcher = vi.fn(async (url: string, options?: RequestInit) => {
    const method = options?.method ?? 'GET';
    if (url.endsWith('/csrf')) return Response.json({ token: 'csrf' });
    if (url === '/api/auth/me') return Response.json({ requiredOfficePercentage: 60 });
    if (method === 'GET') return Response.json({ today: '2026-10-16', items: rows });
    const id = url.split('/').at(-1);
    if (method === 'DELETE') rows = rows.filter(row => row.id !== id);
    else {
      const body = JSON.parse(String(options?.body));
      if (method === 'PUT') rows = rows.map(row => row.id === id ? { ...row, ...body } : row);
      else rows.push({ ...body, id: 'new', isBaseline: false });
    }
    return new Response(null, { status: 204 });
  });
  vi.stubGlobal('fetch', fetcher); return fetcher;
}

it('orders current and past descending, future ascending, and protects the baseline', async () => {
  mockApi(); render(AttendanceRequirements, { onsave: vi.fn() });
  await screen.findByText('Initial requirement · 50%');
  expect(screen.getAllByRole('button', { name: /^Edit / }).map(button => button.getAttribute('aria-label')))
    .toEqual(['Edit January 2026', 'Edit Initial requirement', 'Edit January 2027', 'Edit April 2027']);
  expect(screen.queryByRole('button', { name: 'Delete Initial requirement' })).not.toBeInTheDocument();
  await fireEvent.click(screen.getByRole('button', { name: 'Edit Initial requirement' }));
  expect(screen.queryByLabelText('Effective from')).not.toBeInTheDocument();
});

it('adds a month, edits the baseline and refreshes the current user', async () => {
  const fetcher = mockApi(); const onsave = vi.fn();
  render(AttendanceRequirements, { onsave });
  await screen.findByText('Initial requirement · 50%');
  await fireEvent.click(screen.getByRole('button', { name: 'Add requirement' }));
  await fireEvent.input(screen.getByLabelText('Effective from'), { target: { value: '2026-12' } });
  await fireEvent.input(screen.getByLabelText('Required office days (%)'), { target: { value: '65' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Save requirement' }).closest('form')!);
  await screen.findByText('December 2026 · 65%');
  expect(JSON.parse(String(fetcher.mock.calls.find(([url, options]) => url === path && options?.method === 'POST')![1]?.body)))
    .toEqual({ startingFrom: '2026-12-01', requiredOfficePercentage: 65 });
  await fireEvent.click(screen.getByRole('button', { name: 'Edit Initial requirement' }));
  await fireEvent.input(screen.getByLabelText('Required office days (%)'), { target: { value: '40' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Save requirement' }).closest('form')!);
  await screen.findByText('Initial requirement · 40%');
  expect(onsave).toHaveBeenCalledWith({ requiredOfficePercentage: 60 });
});

it('requires confirmation to delete and explains its effect', async () => {
  const fetcher = mockApi(); render(AttendanceRequirements, { onsave: vi.fn() });
  await screen.findByText('Initial requirement · 50%');
  await fireEvent.click(screen.getByRole('button', { name: 'Delete January 2026' }));
  expect(fetcher.mock.calls.some(([, options]) => options?.method === 'DELETE')).toBe(false);
  expect(screen.getByRole('group', { name: 'Confirm requirement deletion' })).toHaveTextContent('preceding percentage');
  await fireEvent.click(screen.getByRole('button', { name: 'Confirm delete' }));
  await waitFor(() => expect(screen.queryByText('January 2026 · 60%')).not.toBeInTheDocument());
});

it('retains the form on a rejected save and permits retrying a failed load', async () => {
  const fetcher = mockApi(); fetcher.mockRejectedValueOnce(new Error('Connection lost'));
  render(AttendanceRequirements, { onsave: vi.fn() });
  await screen.findByRole('alert');
  await fireEvent.click(screen.getByRole('button', { name: 'Retry loading requirements' }));
  await screen.findByText('Initial requirement · 50%');
  await fireEvent.click(screen.getByRole('button', { name: 'Edit January 2026' }));
  fetcher.mockResolvedValueOnce(Response.json({ token: 'csrf' }));
  fetcher.mockResolvedValueOnce(Response.json({ title: 'A requirement already exists for this month.' }, { status: 409 }));
  await fireEvent.submit(screen.getByRole('button', { name: 'Save requirement' }).closest('form')!);
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('already exists'));
  expect(screen.getByLabelText('Effective from')).toHaveValue('2026-01');
  expect(screen.getByRole('button', { name: 'Save requirement' })).toBeEnabled();
});
