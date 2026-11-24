import { writable } from 'svelte/store';
import { request } from '$lib/api';

export type PublishedPrivacyNotice = { operatorName: string; contactEmail: string; details: string };
type PublicConfiguration = { privacyNotice: PublishedPrivacyNotice | null; persistentLoginSeconds: number };
type ConfigurationState =
  | { status: 'loading' | 'error' }
  | ({ status: 'ready' } & PublicConfiguration);

export const configuration = writable<ConfigurationState>({ status: 'loading' });

export async function loadConfiguration(): Promise<void> {
  configuration.set({ status: 'loading' });
  try {
    const settings = await request<PublicConfiguration>('/api/configuration');
    configuration.set({ status: 'ready', ...settings });
  } catch {
    configuration.set({ status: 'error' });
  }
}
