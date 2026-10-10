import { fireEvent, render, screen, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import PlanningCalendar from './PlanningCalendar.svelte';
import type { PlanningDay, PlanningMonth } from '$lib/types';
import { resetCsrf } from '$lib/api';

function day(date: string, overrides: Partial<PlanningDay> = {}): PlanningDay {
  return { date, isPlanned: false, isEligible: true, isAttended: false, isToday: false,
    canToggle: true, status: 'none', holidays: [], isVacation: false, ...overrides };
}
function setup(days: PlanningDay[], save: () => Promise<Response> = async () => new Response(null, { status: 204 })) {
  resetCsrf();
  const data: PlanningMonth = { today: '2026-10-12', upcomingPlannedDays: 0, days };
  const fetcher = vi.fn(async (path: string, options?: RequestInit) => {
    if (path === '/api/auth/csrf') return new Response('{"token":"test"}');
    if (options?.method === 'PUT' || options?.method === 'DELETE') {
      const result = await save();
      if (result.ok) {
        const date = path.split('/').at(-1);
        data.days = data.days.map(item => item.date === date ? { ...item, isPlanned: options.method === 'PUT', status: options.method === 'PUT' ? 'planned' : 'none' } : item);
        data.upcomingPlannedDays = data.days.filter(item => item.isPlanned && !item.isAttended).length;
      }
      return result;
    }
    return new Response(JSON.stringify(data));
  });
  vi.stubGlobal('fetch', fetcher);
  const oncountchange = vi.fn();
  render(PlanningCalendar, { period: { year: 2026, month: 10 }, timeZone: 'UTC', oncountchange, onchange: vi.fn() });
  return { fetcher, oncountchange };
}

it('toggles immediately, persists without a save button, and rolls the count back on failure', async () => {
  let resolve!: (value: Response) => void;
  const { oncountchange } = setup([day('2026-10-12')], () => new Promise(done => resolve = done));
  const cell = await screen.findByRole('button', { name: /Oct 12.*Toggle plan/ });
  await fireEvent.click(cell);
  expect(cell).toHaveClass('planned');
  expect(oncountchange).toHaveBeenLastCalledWith(1);
  expect(screen.queryByRole('button', { name: /^Save/ })).not.toBeInTheDocument();
  await waitFor(() => expect(resolve).toBeTypeOf('function'));
  resolve(new Response('{"detail":"Could not save plan"}', { status: 500 }));
  await waitFor(() => expect(cell).not.toHaveClass('planned'));
  expect(oncountchange).toHaveBeenLastCalledWith(0);
  expect(screen.getByRole('alert')).toHaveTextContent('Could not save plan');
});

it('adds and removes plans using explicit PUT and DELETE requests', async () => {
  const { fetcher } = setup([day('2026-10-12')]);
  const cell = await screen.findByRole('button', { name: /Oct 12.*Toggle plan/ });
  await fireEvent.click(cell);
  await waitFor(() => expect(cell).toBeEnabled());
  expect(cell).toHaveAttribute('aria-pressed', 'true');
  await fireEvent.click(cell);
  await waitFor(() => expect(cell).toBeEnabled());
  expect(cell).toHaveAttribute('aria-pressed', 'false');
  expect(fetcher.mock.calls.filter(([, options]) => options?.method).map(([, options]) => options?.method)).toEqual(['PUT', 'DELETE']);
});

it('keeps today outline with attendance color and exposes holiday details on touch without changing plans', async () => {
  const { fetcher } = setup([
    day('2026-10-06', { isPlanned: true, status: 'missed', canToggle: false }),
    day('2026-10-12', { isToday: true, isAttended: true, status: 'attended' }),
    day('2026-10-14', { isEligible: false, canToggle: false, status: 'holiday', holidays: ['National holiday'], isVacation: true })
  ]);
  const today = await screen.findByRole('button', { name: /Oct 12/ });
  expect(today).toHaveClass('today', 'attended');
  expect(today).toHaveAttribute('aria-current', 'date');
  const holiday = screen.getByRole('button', { name: /Oct 14/ });
  expect(holiday).toHaveAttribute('title', 'National holiday · Vacation');
  await fireEvent.click(holiday);
  expect(screen.getByText(/Oct 14.*National holiday.*Vacation/)).toBeVisible();
  await fireEvent.click(screen.getByRole('button', { name: /Oct 6/ }));
  expect(fetcher.mock.calls.some(([, options]) => options?.method)).toBe(false);
});
