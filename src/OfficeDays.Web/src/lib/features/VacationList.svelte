<script lang="ts">
  import ItemList from '$lib/components/ItemList.svelte';
  import { untrack } from 'svelte';
  import { onMount } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import { displayDate, isoDate, periodQuery } from '$lib/dates';
  import type { Vacation, Period } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  let { period, onchange }: { period: Period; onchange: () => void } = $props();
  let from = $state(untrack(() => isoDate(period.year, period.month))); let to = $state(untrack(() => isoDate(period.year, period.month)));
  let items = $state<Vacation[]>([]); let loading = $state(true); let busy = $state(false); let error = $state(''); let message = $state('');
  async function load() {
    loading = true; error = '';
    try { items = await request<Vacation[]>(`/api/vacations?${periodQuery(period)}`); }
    catch (failure) { error = errorMessage(failure); }
    finally { loading = false; }
  }
  onMount(() => { void load(); });
  async function change(id?: string) {
    busy = true; error = ''; message = '';
    try {
      await mutate(id ? `/api/vacations/${id}` : '/api/vacations', id ? 'DELETE' : 'POST', id ? undefined : { from, to });
      message = id ? 'Vacation removed.' : 'Vacation added.'; onchange(); await load();
    } catch (failure) { error = errorMessage(failure); }
    finally { busy = false; }
  }
</script>
<section class="card" aria-labelledby="vacation-heading">
  <div class="section-heading"><div><h2 id="vacation-heading">Vacation</h2><p class="muted">Ranges overlap safely and are counted once.</p></div></div>
  <form class="range-form" onsubmit={event => { event.preventDefault(); void change(); }}>
    <label>From<input type="date" required bind:value={from} /></label><label>To<input type="date" required min={from} bind:value={to} /></label>
    <button disabled={busy} type="submit">Add</button>
  </form>
  <Feedback {error} {message} />
  {#if error}<button class="secondary small" onclick={load} disabled={loading}>Retry vacations</button>{/if}
  <ItemList {loading}>
    {#if loading && !items.length}<li class="empty">Loading vacations…</li>
    {:else}{#each items as item (item.id)}
      <li><div class="item-main"><strong>{displayDate(item.from)}{item.from === item.to ? '' : ` – ${displayDate(item.to)}`}</strong></div><button class="danger small" disabled={busy} aria-label={`Remove vacation ${displayDate(item.from)}`} onclick={() => change(item.id)}>Remove</button></li>
    {:else}<li class="empty">No vacation overlapping this month.</li>{/each}{/if}
  </ItemList>
</section>

<style>
  .range-form { display: grid; grid-template-columns: 1fr 1fr auto; gap: .55rem; align-items: end; }
  @media (max-width: 650px) {
    .range-form { grid-template-columns: 1fr 1fr; }
    .range-form button { grid-column: 1 / -1; }
  }
</style>
