import { writable } from 'svelte/store';
import { ApiError, onUnauthorized, request, resetCsrf } from './api';
import type { User } from './types';

export const user = writable<User | null>(null);
export const sessionReady = writable(false);
export const sessionError = writable('');
onUnauthorized(() => user.set(null));

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
  resetCsrf(); 
  user.set(value); 
  sessionReady.set(true); 
}

export function signedOut(): void { 
  resetCsrf(); 
  user.set(null); 
}
