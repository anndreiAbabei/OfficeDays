import { render, screen, waitFor } from '@testing-library/svelte';
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
  expect(fetcher.mock.calls.map(([path]) => path).sort()).toEqual(['/api/attendance?year=2026&month=10', '/api/status?year=2026&month=10', '/api/vacations?year=2026&month=10']);
});
it('keeps attendance usable when the summary endpoint fails', async () => {
  vi.stubGlobal('fetch', vi.fn().mockImplementation(async (path: string) => path.startsWith('/api/status')
    ? new Response(JSON.stringify({ detail: 'Summary unavailable' }), { status: 503 })
    : new Response('[]')));
  render(Dashboard, { period: { year: 2026, month: 10 }, timeZone: 'UTC' });
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Summary unavailable'));
  expect(screen.getByRole('button', { name: 'Retry summary' })).toBeEnabled();
  expect(screen.getByRole('button', { name: 'Add day' })).toBeEnabled();
  expect(screen.getByText('No office days recorded for this month.')).toBeInTheDocument();
});
