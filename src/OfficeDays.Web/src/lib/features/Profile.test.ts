import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import Profile from './Profile.svelte';
import type { User } from '$lib/types';
import { resetCsrf } from '$lib/api';
const user: User = { id: '1', username: 'Andrei', email: 'old@example.com', timeZoneId: 'UTC', countryCode: 'RO', countryName: 'Romania', isAdmin: false, teamDay: null, requiredOfficePercentage: 50 };

it('clears optional email and saves the percentage, disabling duplicate submissions', async () => {
  resetCsrf(); const onsave = vi.fn();
  const fetcher = vi.fn().mockImplementation(async (path: string) => new Response(JSON.stringify(path.endsWith('csrf') ? { token: 'csrf' } : { ...user, email: null, requiredOfficePercentage: 60 })));
  vi.stubGlobal('fetch', fetcher);
  
  render(Profile, { currentUser: user, onsave });
  
  await fireEvent.input(screen.getByLabelText('Email address'), { target: { value: '' } });
  await fireEvent.input(screen.getByLabelText('Required office days (%)'), { target: { value: '60' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Save profile' }).closest('form')!);
  await waitFor(() => expect(onsave).toHaveBeenCalledWith({ ...user, email: null, requiredOfficePercentage: 60 }));
  
  expect(JSON.parse(fetcher.mock.calls.find(([path]) => path === '/api/users/me')![1].body)).toEqual({ email: null, requiredOfficePercentage: 60, teamDay: null });
  expect(screen.getByRole('status')).toHaveTextContent('Profile saved.');
});
it('keeps entered values and exposes a failed save for retry', async () => {
  resetCsrf(); vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('Connection lost')));
  
  render(Profile, { currentUser: user, onsave: vi.fn() });
  
  await fireEvent.submit(screen.getByRole('button', { name: 'Save profile' }).closest('form')!);
  await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Connection lost'));
  
  expect(screen.getByRole('button', { name: 'Save profile' })).toBeEnabled();
  expect(screen.getByLabelText('Email address')).toHaveValue('old@example.com');
});

it('loads, changes and clears the team day through the profile save', async () => {
  resetCsrf();
  const fetcher = vi.fn(async (path: string, options?: RequestInit) =>
    new Response(JSON.stringify(path.endsWith('csrf') ? { token: 'csrf' } : { ...user, ...JSON.parse(String(options?.body)) })));
  vi.stubGlobal('fetch', fetcher);
  const onsave = vi.fn();
  render(Profile, { currentUser: { ...user, teamDay: 'thursday' }, onsave });
  const select = screen.getByLabelText('Team day');
  expect(select).toHaveValue('thursday');
  expect(screen.getAllByRole('option')).toHaveLength(6);
  await fireEvent.change(select, { target: { value: 'monday' } });
  await fireEvent.submit(select.closest('form')!);
  await waitFor(() => expect(onsave).toHaveBeenLastCalledWith({ ...user, teamDay: 'monday' }));
  await fireEvent.change(select, { target: { selectedIndex: 0 } });
  await fireEvent.submit(select.closest('form')!);
  await waitFor(() => expect(onsave).toHaveBeenLastCalledWith({ ...user, teamDay: null }));
});
