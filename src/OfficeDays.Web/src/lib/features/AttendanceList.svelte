<script lang="ts">
  import { untrack } from 'svelte';
  import { onMount } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import { displayDate, isoDate, localDate, periodQuery } from '$lib/dates';
  import type { Attendance, Period } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  let { period, timeZone, onchange }: { period: Period; timeZone: string; onchange: () => void } = $props();
  const today = untrack(() => localDate(timeZone));
  const maxDate = isoDate(today.year, today.month, today.day);
  let date = $state(untrack(() => period.year === today.year && period.month === today.month ? maxDate : isoDate(period.year, period.month)));
  let items = $state<Attendance[]>([]); let loading = $state(true); let busy = $state(false); let error = $state(''); let message = $state('');
  async function load() {
    loading = true; error = '';
    try { items = await request<Attendance[]>(`/api/attendance?${periodQuery(period)}`); }
    catch (failure) { error = errorMessage(failure); }
    finally { loading = false; }
  }
  onMount(() => { void load(); });
  async function change(method: string, value: string) {
    busy = true; error = ''; message = '';
    try {
      await mutate(`/api/attendance/${value}`, method);
      message = method === 'PUT' ? 'Office day recorded.' : 'Office day removed.';
      onchange(); await load();
    } catch (failure) { error = errorMessage(failure); }
    finally { busy = false; }
  }
</script>
<section class="card" aria-labelledby="attendance-heading">
  <div class="section-heading"><div><h2 id="attendance-heading">Office days</h2><p class="muted">Add a missed day or remove a mistake.</p></div></div>
  <form class="inline-form" onsubmit={event => { event.preventDefault(); void change('PUT', date); }}>
    <input type="date" required max={maxDate} bind:value={date} aria-label="Office attendance date" />
    <button disabled={busy} type="submit">Add day</button>
  </form>
  <Feedback {error} {message} />
  {#if error}<button class="secondary small" onclick={load} disabled={loading}>Retry office days</button>{/if}
  <ul class="item-list attendance-list" aria-busy={loading}>
    {#if loading && !items.length}<li class="empty">Loading office days…</li>
    {:else}
      {#each items as item (item.date)}
        <li><div class="item-main"><strong title={item.isManual ? 'Manually added' : 'Automatically added'}>
          <span class:manual={item.isManual} class:automatic={!item.isManual} class="attendance-check" aria-hidden="true">✓ </span>{displayDate(item.date)}
          <span class="sr-only"> · {item.isManual ? 'Manually added' : 'Automatically added'}</span>
        </strong></div><button class="danger small" disabled={busy} aria-label={`Remove office day ${displayDate(item.date)}`} onclick={() => change('DELETE', item.date)}>Remove</button></li>
      {:else}<li class="empty">No office days recorded for this month.</li>{/each}
    {/if}
  </ul>
</section>
