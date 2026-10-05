import { beforeEach, expect, it, vi } from 'vitest';
import { mutate, request, onUnauthorized, resetCsrf, ApiError } from './api';
const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status });
beforeEach(() => { resetCsrf(); onUnauthorized(() => {}); });
it('shares CSRF loading for concurrent writes and sends same-origin cookies', async () => {
  const fetcher = vi.fn().mockResolvedValueOnce(json({ token: 'csrf-one' })).mockResolvedValue(new Response(null, { status: 204 }));
  vi.stubGlobal('fetch', fetcher);
  await Promise.all([mutate('/api/attendance/2026-10-01', 'PUT'), mutate('/api/vacations/123', 'DELETE')]);
  expect(fetcher.mock.calls.filter(([path]) => path === '/api/auth/csrf')).toHaveLength(1);
  for (const [, options] of fetcher.mock.calls.slice(1)) {
    expect(options.credentials).toBe('same-origin');
    expect(options.headers.get('X-CSRF-TOKEN')).toBe('csrf-one');
  }
});
it('refreshes CSRF after identity changes', async () => {
  const fetcher = vi.fn().mockImplementation(async (path: string) => path.endsWith('/csrf') ? json({ token: 'token' }) : new Response(null, { status: 204 }));
  vi.stubGlobal('fetch', fetcher);
  await mutate('/api/auth/login', 'POST', {}); resetCsrf(); await mutate('/api/users/me', 'PUT', {});
  expect(fetcher.mock.calls.filter(([path]) => path.endsWith('/csrf'))).toHaveLength(2);
});
it('surfaces validation errors and does not invalidate a session on a failed login', async () => {
  const unauthorized = vi.fn(); onUnauthorized(unauthorized);
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(json({ errors: { email: ['Enter a valid email.'] } }, 400)));
  await expect(request('/api/users/me')).rejects.toThrow('Enter a valid email.');
  vi.stubGlobal('fetch', vi.fn().mockResolvedValue(json({ title: 'Invalid credentials' }, 401)));
  await expect(request('/api/auth/login')).rejects.toBeInstanceOf(ApiError);
  expect(unauthorized).not.toHaveBeenCalled();
  await expect(request('/api/status')).rejects.toThrow(); expect(unauthorized).toHaveBeenCalledOnce();
});
it('allows retry when CSRF loading fails', async () => {
  const fetcher = vi.fn().mockRejectedValueOnce(new Error('Offline')).mockResolvedValueOnce(json({ token: 'retry' })).mockResolvedValueOnce(new Response(null, { status: 204 }));
  vi.stubGlobal('fetch', fetcher);
  await expect(mutate('/api/auth/logout', 'POST')).rejects.toThrow('Offline');
  await expect(mutate('/api/auth/logout', 'POST')).resolves.toBeUndefined();
});
