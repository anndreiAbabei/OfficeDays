<script lang="ts">
  import ItemList from '$lib/components/ItemList.svelte';
  import { onMount } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import type { Jurisdiction } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  import Holidays from './Holidays.svelte';
  
  let { countryCode, timeZone }: { countryCode: string; timeZone: string } = $props();
  let items = $state<Jurisdiction[]>([]); let code = $state(''); let name = $state('');
  let busy = $state(false); let loading = $state(true); let error = $state(''); let message = $state('');
  
  async function load() {
    loading = true; error = '';
    try { items = await request<Jurisdiction[]>('/api/holiday-jurisdictions'); }
    catch (failure) { error = errorMessage(failure); }
    finally { loading = false; }
  }
  
  onMount(() => { void load(); });
  
  async function change(method: string, item?: Jurisdiction) {
    let body: unknown = { code, name };
    if (method === 'DELETE') { if (!confirm(`Remove ${item!.name}?`)) return; body = undefined; }
    if (method === 'PUT') {
      const updatedName = prompt('Jurisdiction name', item!.name);
      if (!updatedName?.trim() || updatedName.trim() === item!.name) return;
      body = { name: updatedName.trim() };
    }
    busy = true; error = ''; message = '';
    try {
      await mutate(`/api/holiday-jurisdictions${item ? `/${encodeURIComponent(item.code)}` : ''}`, method, body);
      if (method === 'POST') { code = ''; name = ''; }
      message = method === 'DELETE' ? 'Holiday jurisdiction removed.' : 'Holiday jurisdiction saved.'; await load();
    } catch (failure) { error = errorMessage(failure); }
    finally { busy = false; }
  }
</script>
<div class="admin-sections">
  <section class="card">
    <div class="section-heading">
      <div>
        <h2>Holiday jurisdictions</h2>
        <p class="muted">Countries or regional holiday calendars.</p>
      </div>
    </div>
    
    <form class="jurisdiction-form" onsubmit={event => { event.preventDefault(); void change('POST'); }}>
      <label>
        Code
        <input maxlength="6" placeholder="RO or GB-NIR" required bind:value={code} />
      </label>
      <label>
        Name
        <input maxlength="100" placeholder="Jurisdiction name" required bind:value={name} />
      </label>
      <button type="submit" disabled={busy}>Add jurisdiction</button>
    </form>
    
    <Feedback {error} {message} />
    {#if error}
      <button class="secondary small" onclick={load} disabled={loading}>
        Retry jurisdictions
      </button>
    {/if}
    
    <ItemList {loading}>
      {#if loading && !items.length}
        <li class="empty">Loading jurisdictions…</li>
      {:else}
        {#each items as item (item.code)}
          <li>
            <div class="item-main">
              <strong>{item.name}</strong>
              <small>{item.code}</small>
            </div>
            <div class="row">
              <button class="secondary small" disabled={busy} onclick={() => change('PUT', item)}>
                Rename
              </button>
              <button class="danger small" disabled={busy} onclick={() => change('DELETE', item)}>
                Remove
              </button>
            </div>
          </li>
        {:else}
          <li class="empty">No holiday jurisdictions configured.</li>
        {/each}
      {/if}
    </ItemList>
  </section>
    
  <Holidays jurisdictions={items} {countryCode} {timeZone} />
</div>

<style>
  .admin-sections > :global(*) { 
    min-width: 0; 
  }
  .admin-sections { 
    display: grid;
    gap: 1rem; 
  }
  .jurisdiction-form { 
    display: grid; 
    gap: .55rem; 
    align-items: end; 
    margin-top: 1rem; 
  }
  @media (min-width: 651px) {
    .jurisdiction-form { grid-template-columns: minmax(6rem, 1fr) minmax(0, 2fr) auto; }
  }
</style>
