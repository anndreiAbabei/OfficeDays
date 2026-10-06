<script lang="ts">
  import ItemList from '$lib/components/ItemList.svelte';
  import { untrack } from 'svelte';
  import { beforeNavigate } from '$app/navigation';
  import { request, mutate, errorMessage } from '$lib/api';
  import { displayDate, localDate } from '$lib/dates';
  import type { Holiday, Jurisdiction } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  let { jurisdictions, countryCode, timeZone }: { jurisdictions: Jurisdiction[]; countryCode: string; timeZone: string } = $props();
  let selected = $state(untrack(() => countryCode)); let year = $state(untrack(() => localDate(timeZone).year));
  let items = $state<Holiday[]>([]); let date = $state(''); let name = $state(''); let dirty = $state(false);
  let loaded = $state(false); let loading = $state(false); let busy = $state(false); let error = $state(''); let message = $state('');
  let revision = 0;
  $effect(() => {
    if (jurisdictions.length && !jurisdictions.some(item => item.code === selected)) selected = jurisdictions[0].code;
  });
  let validSelection = $derived(jurisdictions.some(item => item.code === selected));
  $effect(() => { if (validSelection) void load(selected, year); });
  beforeNavigate(({ cancel }) => { if (dirty && !confirm('Discard unsaved bank holiday changes?')) cancel(); });
  function beforeUnload(event: BeforeUnloadEvent) { if (dirty) { event.preventDefault(); event.returnValue = ''; } }
  async function load(code: string, targetYear: number) {
    const current = ++revision; loading = true; loaded = false; error = ''; items = []; dirty = false;
    try {
      const result = await request<Holiday[]>(`/api/bank-holidays/${encodeURIComponent(code)}/${targetYear}`);
      if (current === revision) { items = result; loaded = true; }
    } catch (failure) { if (current === revision) error = errorMessage(failure); }
    finally { if (current === revision) loading = false; }
  }
  function selectPeriod(event: Event, field: 'code' | 'year') {
    const input = event.currentTarget as HTMLInputElement;
    if ((dirty && !confirm('Discard unsaved bank holiday changes?')) || !input.checkValidity() || !input.value) {
      input.value = field === 'code' ? selected : String(year); return;
    }
    dirty = false; message = ''; date = ''; name = '';
    if (field === 'code') selected = input.value; else year = Number(input.value);
  }
  function add(event: SubmitEvent) {
    event.preventDefault(); error = '';
    if (Number(date.slice(0, 4)) !== year) { error = 'Holiday must be in the selected year.'; return; }
    if (!name.trim()) { error = 'Enter a holiday name.'; return; }
    items = [...items.filter(item => item.date !== date), { date, name: name.trim() }].sort((a, b) => a.date.localeCompare(b.date));
    name = ''; dirty = true; message = 'Changes are local. Save the year to apply.';
  }
  async function save() {
    busy = true; error = ''; message = '';
    try { await mutate(`/api/bank-holidays/${encodeURIComponent(selected)}/${year}`, 'PUT', items); dirty = false; message = 'Bank holidays saved.'; }
    catch (failure) { error = errorMessage(failure); }
    finally { busy = false; }
  }
</script>
<svelte:window onbeforeunload={beforeUnload} />
<section class="card">
  <h2>Bank holidays</h2><p class="muted">Saving replaces the complete selected year.</p>
  <div class="holiday-picker">
    <label>Jurisdiction<select value={selected} disabled={busy} onchange={event => selectPeriod(event, 'code')}>{#each jurisdictions as item}<option value={item.code}>{item.name} ({item.code})</option>{/each}</select></label>
    <label>Year<input type="number" min="1" max="9999" required value={year} disabled={busy} onchange={event => selectPeriod(event, 'year')} /></label>
    <button disabled={busy || loading || !loaded || !dirty} onclick={save}>{busy ? 'Saving…' : 'Save year'}</button>
  </div>
  <form class="holiday-form" onsubmit={add}>
    <label>Date<input type="date" required bind:value={date} /></label><label>Holiday name<input maxlength="150" required bind:value={name} /></label>
    <button type="submit" class="secondary" disabled={!loaded || loading || busy}>Add / update</button>
  </form>
  <Feedback {error} {message} />
  {#if error && !loaded}<button class="secondary small" onclick={() => load(selected, year)} disabled={loading}>Retry holidays</button>{/if}
  <ItemList {loading}>
    {#if loading}<li class="empty">Loading bank holidays…</li>
    {:else}{#each items as item (item.date)}
      <li><div class="item-main"><strong>{displayDate(item.date)}</strong><small>{item.name}</small></div><div class="row">
        <button class="secondary small" disabled={busy} onclick={() => { date = item.date; name = item.name; }}>Edit</button>
        <button class="danger small" disabled={busy} onclick={() => { items = items.filter(row => row.date !== item.date); dirty = true; message = 'Changes are local. Save the year to apply.'; }}>Remove</button>
      </div></li>
    {:else}<li class="empty">No configured holidays for this year.</li>{/each}{/if}
</ItemList>
</section>

<style>
  .holiday-form, .holiday-picker { display: grid; gap: .55rem; align-items: end; margin-top: 1rem; }
  .holiday-picker { margin-bottom: 1rem; }
  @media (min-width: 651px) {
    .holiday-form, .holiday-picker { grid-template-columns: minmax(0, 1fr) minmax(0, 1fr) auto; }
  }
</style>
