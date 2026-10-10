import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { beforeEach, expect, it, vi } from 'vitest';
import AttendanceList from './AttendanceList.svelte';
import VacationList from './VacationList.svelte';
import { resetCsrf } from '$lib/api';
const period = { year: 2026, month: 9 };
const json = (value: unknown, status = 200) => new Response(JSON.stringify(value), { status });

beforeEach(resetCsrf);

it('distinguishes attendance sources and retains a failed removal for retry', async () => {
  const onchange = vi.fn();
  
  vi.stubGlobal('fetch', vi.fn().mockImplementation(async (path: string, options: RequestInit) => {
    if (path.endsWith('/csrf')) 
      return json({ token: 'csrf' });
    
    if (options.method === 'DELETE') 
      return json({ detail: 'Could not remove this day.' }, 500);
    
    return json([{ date: '2026-09-01', isManual: true }, { date: '2026-09-02', isManual: false }]);
  }));
  
  render(AttendanceList, { period, timeZone: 'UTC', onchange });
  
  await waitFor(() => expect(screen.getAllByRole('button', { name: /Remove office day/ })).toHaveLength(2));
  
  expect(screen.getByTitle('Manually added')).toBeInTheDocument();
  expect(screen.getByTitle('Automatically added')).toBeInTheDocument();
  await fireEvent.click(screen.getAllByRole('button', { name: /Remove office day/ })[0]);
 
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Could not remove this day.'));
  
  expect(screen.getAllByRole('button', { name: /Remove office day/ })).toHaveLength(2);
  expect(onchange).not.toHaveBeenCalled();
});

it('records a manual day and refreshes the summary only after a successful write', async () => {
  const onchange = vi.fn();
  const fetcher = vi.fn().mockImplementation(async (path: string, options: RequestInit) => {
    if (path.endsWith('/csrf')) 
      return json({ token: 'csrf' });
    
    return options.method === 'PUT' ? json({}) : json([]);
  });
  vi.stubGlobal('fetch', fetcher);
  
  render(AttendanceList, { period, timeZone: 'UTC', onchange });
  
  await fireEvent.input(screen.getByLabelText('Office attendance date'), { target: { value: '2026-09-04' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Add day' }).closest('form')!);
  await waitFor(() => expect(onchange).toHaveBeenCalledOnce());
  
  expect(fetcher.mock.calls.some(([path, options]) => path === '/api/attendance/2026-09-04' && options.method === 'PUT')).toBe(true);
});

it('posts a vacation range and refreshes only after the API accepts it', async () => {
  const onchange = vi.fn();
  const fetcher = vi.fn().mockImplementation(async (path: string, options: RequestInit) => {
    if (path.endsWith('/csrf')) 
      return json({ token: 'csrf' });
    
    return options.method === 'POST' ? json({}) : json([]);
  });
  vi.stubGlobal('fetch', fetcher);
  
  render(VacationList, { period, onchange });
  
  await fireEvent.input(screen.getByLabelText('From'), { target: { value: '2026-09-10' } });
  await fireEvent.input(screen.getByLabelText('To'), { target: { value: '2026-09-14' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Add' }).closest('form')!);
  await waitFor(() => expect(onchange).toHaveBeenCalledOnce());
  
  expect(JSON.parse(fetcher.mock.calls.find(([, options]) => options.method === 'POST')![1].body)).toEqual({ from: '2026-09-10', to: '2026-09-14' });
});
