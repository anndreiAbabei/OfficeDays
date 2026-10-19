import { resetCsrf } from '$lib/api';
import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import Dashboard from './Dashboard.svelte';

it('renders summary while secondary requests remain pending; never requests tokens or admin data', async () => {
  const fetcher = vi.fn().mockImplementation((path: string) => path.startsWith('/api/status')
    ? Promise.resolve(new Response(JSON.stringify({ period: '2026-10', requiredOfficeDays: 12, officeDays: 5, remainingOfficeDays: 7, eligibleWorkingDays: 23, maximumWfhDays: 11, progressPercentage: 41.67 })))
    : new Promise(() => {}));
  vi.stubGlobal('fetch', fetcher);
  
  render(Dashboard, { period: { year: 2026, month: 10 }, timeZone: 'UTC' });
  
  await waitFor(() => expect(screen.getByText('12')).toBeInTheDocument());
  expect(screen.getByText('Loading office days…')).toBeInTheDocument();
  expect(screen.getByText('Loading vacations…')).toBeInTheDocument();
  expect(fetcher.mock.calls.map(([path]) => path).sort()).toEqual(['/api/attendance?year=2026&month=10', '/api/planning?year=2026&month=10', '/api/status?year=2026&month=10', '/api/vacations?year=2026&month=10']);
});
it('keeps attendance usable when the summary endpoint fails', async () => {
  vi.stubGlobal('fetch', vi.fn().mockImplementation(async (path: string) => path.startsWith('/api/status')
    ? new Response(JSON.stringify({ detail: 'Summary unavailable' }), { status: 503 })
    : new Response(path.startsWith('/api/planning') ? JSON.stringify({ today: '2026-10-10', upcomingPlannedDays: 0, days: [] }) : '[]')));
  
  render(Dashboard, { period: { year: 2026, month: 10 }, timeZone: 'UTC' });
  
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Summary unavailable'));
  expect(screen.getByRole('button', { name: 'Retry summary' })).toBeEnabled();
  expect(screen.getByRole('button', { name: 'Add day' })).toBeEnabled();
  expect(screen.getByText('No office days recorded for this month.')).toBeInTheDocument();
});

it('refreshes vacation dates, calendar, planning count and summary after a vacation edit', async () => {
  resetCsrf();
  let saved = false;
  const fetcher = vi.fn(async (path: string, options?: RequestInit) => {
    if (path.endsWith('/csrf')) return Response.json({ token: 'csrf' });
    if (options?.method === 'PUT' && path === '/api/vacations/vacation-1') {
      saved = true;
      return new Response(null, { status: 204 });
    }
    if (path.startsWith('/api/status')) return Response.json({
      period: '2026-09', requiredOfficeDays: 10, officeDays: 4, remainingOfficeDays: 6,
      eligibleWorkingDays: saved ? 20 : 18, maximumWfhDays: 10, progressPercentage: 40,
      upcomingPlannedDays: saved ? 1 : 0
    });
    if (path.startsWith('/api/vacations')) return Response.json([
      { id: 'vacation-1', from: saved ? '2026-09-22' : '2026-09-21', to: saved ? '2026-09-22' : '2026-09-25' }
    ]);
    if (path.startsWith('/api/planning')) return Response.json({
      today: '2026-09-15', upcomingPlannedDays: saved ? 1 : 0,
      days: [{ date: '2026-09-21', isPlanned: true, isEligible: saved, isAttended: false,
        isToday: false, canToggle: saved, status: saved ? 'planned' : 'holiday', holidays: [], isVacation: !saved }]
    });
    return Response.json([]);
  });
  vi.stubGlobal('fetch', fetcher);
  render(Dashboard, { period: { year: 2026, month: 9 }, timeZone: 'UTC' });
  await screen.findByRole('button', { name: /Sep 21.*Vacation/ });
  await fireEvent.click(await screen.findByRole('button', { name: /Edit vacation/ }));
  await fireEvent.input(screen.getByLabelText('Edit from'), { target: { value: '2026-09-22' } });
  await fireEvent.input(screen.getByLabelText('Edit to'), { target: { value: '2026-09-22' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Save' }).closest('form')!);
  await screen.findByRole('button', { name: /Edit vacation Sep 22/ });
  await screen.findByRole('button', { name: /Sep 21.*Toggle plan/ });
  await screen.findByLabelText('1 planned / 6 remaining office days');
  await screen.findByText(/20 eligible working days/);
  expect(fetcher.mock.calls.filter(([path]) => path.startsWith('/api/status'))).toHaveLength(2);
  expect(fetcher.mock.calls.filter(([path]) => path.startsWith('/api/planning'))).toHaveLength(2);
});
