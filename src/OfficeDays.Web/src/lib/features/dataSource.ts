import { getContext, setContext } from 'svelte';
import { request, mutate } from '$lib/api';

// Scoped to a component tree: guest operations never change signed-in transport.
export type DataSource = { request: typeof request; mutate: typeof mutate };
const key = Symbol('personal-data-source');
export function provideDataSource(source: DataSource): void { setContext(key, source); }
export function useDataSource(): DataSource { return getContext<DataSource>(key) ?? { request, mutate }; }
