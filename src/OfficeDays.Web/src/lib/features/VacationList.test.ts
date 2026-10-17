import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import VacationList from './VacationList.svelte';
import { resetCsrf } from '$lib/api';

function setup(fail = false, moveOutOfMonth = false) {
  resetCsrf();
  let vacation = { id: 'vacation-1', from: '2026-09-21', to: '2026-09-25' };
  let saved = false;
  const fetcher = vi.fn(async (path: string, options?: RequestInit) => {
    if (path.endsWith('/csrf')) return Response.json({ token: 'csrf' });
    if (options?.method === 'PUT') {
      if (fail) return Response.json({ detail: 'Could not update vacation.' }, { status: 500 });
      vacation = { ...vacation, ...JSON.parse(String(options.body)) };
      saved = true;
      return new Response(null, { status: 204 });
    }
    return Response.json(saved && moveOutOfMonth ? [] : [vacation]);
  });
  vi.stubGlobal('fetch', fetcher);
  const onchange = vi.fn();
  render(VacationList, { period: { year: 2026, month: 9 }, onchange });
  return { fetcher, onchange };
}

it('edits existing dates and refreshes the list and dependent data after saving', async () => {
  const { fetcher, onchange } = setup();
  await fireEvent.click(await screen.findByRole('button', { name: /Edit vacation/ }));
  expect(screen.getByLabelText('Edit from')).toHaveValue('2026-09-21');
  expect(screen.getByLabelText('Edit to')).toHaveValue('2026-09-25');
  await fireEvent.input(screen.getByLabelText('Edit from'), { target: { value: '2026-09-22' } });
  await fireEvent.input(screen.getByLabelText('Edit to'), { target: { value: '2026-09-24' } });
  expect(screen.getByLabelText('Edit to')).toHaveAttribute('min', '2026-09-22');
  await fireEvent.submit(screen.getByRole('button', { name: 'Save' }).closest('form')!);
  await screen.findByRole('button', { name: /Edit vacation Sep 22/ });
  const writes = fetcher.mock.calls.filter(([, options]) => options?.method === 'PUT');
  expect(writes).toHaveLength(1);
  expect(writes[0][0]).toBe('/api/vacations/vacation-1');
  expect(JSON.parse(String(writes[0][1]?.body))).toEqual({ from: '2026-09-22', to: '2026-09-24' });
  expect(onchange).toHaveBeenCalledOnce();
  expect(screen.getByRole('status')).toHaveTextContent('Vacation updated.');
});

it('cancels without changing dates or refreshing dependent data', async () => {
  const { fetcher, onchange } = setup();
  await fireEvent.click(await screen.findByRole('button', { name: /Edit vacation/ }));
  await fireEvent.input(screen.getByLabelText('Edit from'), { target: { value: '2026-09-22' } });
  await fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
  expect(screen.queryByLabelText('Edit from')).not.toBeInTheDocument();
  expect(fetcher.mock.calls.some(([, options]) => options?.method === 'PUT')).toBe(false);
  expect(onchange).not.toHaveBeenCalled();
  await fireEvent.click(screen.getByRole('button', { name: /Edit vacation/ }));
  expect(screen.getByLabelText('Edit from')).toHaveValue('2026-09-21');
});

it('retains entered dates and skips refresh when saving fails', async () => {
  const { onchange } = setup(true);
  await fireEvent.click(await screen.findByRole('button', { name: /Edit vacation/ }));
  await fireEvent.input(screen.getByLabelText('Edit to'), { target: { value: '2026-09-28' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Save' }).closest('form')!);
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Could not update vacation.'));
  expect(screen.getByLabelText('Edit to')).toHaveValue('2026-09-28');
  expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  expect(onchange).not.toHaveBeenCalled();
});

it('removes a vacation from the current month after it is moved to another month', async () => {
  const { onchange } = setup(false, true);
  await fireEvent.click(await screen.findByRole('button', { name: /Edit vacation/ }));
  await fireEvent.input(screen.getByLabelText('Edit from'), { target: { value: '2026-10-05' } });
  await fireEvent.input(screen.getByLabelText('Edit to'), { target: { value: '2026-10-09' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Save' }).closest('form')!);
  await screen.findByText('No vacation overlapping this month.');
  expect(onchange).toHaveBeenCalledOnce();
});
