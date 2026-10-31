import { get, writable } from 'svelte/store';
import { ApiError, onUnauthorized, request, resetCsrf } from './api';
import type { User } from './types';

export const user = writable<User | null>(null);
export const sessionReady = writable(false);
export const sessionMessage = writable('');
export const sessionError = writable('');
let sessionRevision = 0;
onUnauthorized(signedOut);

export async function loadSession(): Promise<void> {
  sessionReady.set(false);
  sessionError.set('');
  
  try { 
    user.set(await request<User>('/api/auth/me')); 
  } catch (error) {
    user.set(null);
    if (!(error instanceof ApiError && error.status === 401)) 
      sessionError.set('Unable to connect. Please try again.');
  } finally { 
    sessionReady.set(true); 
  }
}

export function signedIn(value: User): void {
  sessionRevision++;
  sessionMessage.set('');
  resetCsrf(); 
  user.set(value); 
  sessionReady.set(true); 
}

export function signedOut(): void {
  sessionRevision++;
  resetCsrf(); 
  user.set(null); 
}

// Profile responses can arrive after sign-out, or even after another sign-in.
// They may update only the session that originally opened the profile page.
export function createSessionUpdater(): (value: User) => void {
  const revision = sessionRevision;
  const userId = get(user)?.id;
  return (value: User): void => {
    if (revision === sessionRevision && userId === value.id && get(user)?.id === userId)
      user.set(value);
  };
}
