<script lang="ts">
  import { onMount } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import type { User } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';

  type Requirement = { id: string; startingFrom: string; requiredOfficePercentage: number; isBaseline: boolean };
  type History = { today: string; items: Requirement[] };
  
  let { onsave }: { onsave: (user: User) => void } = $props();
  let history = $state<History | null>(null);
  let loading = $state(true); let busy = $state(false);
  let error = $state(''); let message = $state('');
  let editing = $state<Requirement | null>(null); let formOpen = $state(false);
  let month = $state(''); let percentage = $state<number | undefined>(50);
  let deleting = $state<Requirement | null>(null);
  const path = '/api/users/me/requirements';
  let currentMonth = $derived(history ? `${history.today.slice(0, 7)}-01` : '');
  let past = $derived(history?.items.filter(x => x.startingFrom <= currentMonth).sort((a, b) => b.startingFrom.localeCompare(a.startingFrom)) ?? []);
  let upcoming = $derived(history?.items.filter(x => x.startingFrom > currentMonth).sort((a, b) => a.startingFrom.localeCompare(b.startingFrom)) ?? []);

  function label(row: Requirement): string {
    if (row.isBaseline) return 'Initial requirement';
    const [year, month] = row.startingFrom.split('-').map(Number);
    return `${new Intl.DateTimeFormat('en', { month: 'long', timeZone: 'UTC' }).format(new Date(Date.UTC(2000, month - 1, 1)))} ${year}`;
  }
  
  async function load(): Promise<void> {
    loading = true; error = '';
    try { history = await request<History>(path); }
    catch (failure) { error = errorMessage(failure); }
    finally { loading = false; }
  }
  onMount(load);

  function open(row: Requirement | null): void {
    editing = row; formOpen = true; deleting = null; error = ''; message = '';
    month = row && !row.isBaseline ? row.startingFrom.slice(0, 7) : history!.today.slice(0, 7);
    percentage = row?.requiredOfficePercentage ?? past[0]?.requiredOfficePercentage ?? 50;
  }
  
  async function refresh(): Promise<void> {
    history = await request<History>(path);
    onsave(await request<User>('/api/auth/me'));
  }
  
  async function save(event: SubmitEvent): Promise<void> {
    event.preventDefault(); 
    
    if (busy) 
      return;
    
    busy = true; 
    error = ''; 
    message = '';
    let saved = false;
    
    try {
      await mutate(editing ? `${path}/${editing.id}` : path, editing ? 'PUT' : 'POST', {
        startingFrom: editing?.isBaseline ? editing.startingFrom : `${month}-01`, requiredOfficePercentage: percentage
      });
      
      saved = true; 
      formOpen = false;
      
      await refresh(); 
      
      message = 'Attendance requirement saved.';
    } catch (failure) {
      error = `${saved ? 'Saved, but could not refresh. Reload this page. ' : ''}${errorMessage(failure)}`; 
    }
    finally { 
      busy = false; 
    }
  }
  
  async function remove(): Promise<void> {
    if (!deleting || busy) 
      return;
    
    busy = true; 
    error = ''; 
    message = ''; 
    let saved = false;
    
    try {
      await mutate(`${path}/${deleting.id}`, 'DELETE'); 
      saved = true; 
      deleting = null;
      
      await refresh(); 
      
      message = 'Attendance requirement deleted.';
    } catch (failure) { 
      error = `${saved ? 'Deleted, but could not refresh. Reload this page. ' : ''}${errorMessage(failure)}`; 
    }
    finally { 
      busy = false; 
    }
  }
</script>

<section class="card" aria-busy={busy || loading}>
  <h2>Attendance requirements</h2>
  <p class="muted">
    Requirements apply from the selected month until the next change. 
    Changes to past requirements will update your dashboard calculations and newly generated reports. 
    Previously downloaded CSV files will remain unchanged.
  </p>
  {#if loading}
    <p role="status">Loading attendance requirements…</p>
  {:else if !history}
    <button type="button" onclick={load}>
      Retry loading requirements
    </button>
  {:else}
    <h3>Current and past requirements</h3>
    {#each past as row, index (row.id)}
      {@render entry(row, index === 0 ? 'Current' : 'Previous')}
    {/each}
    
    <h3>Upcoming changes</h3>
    {#each upcoming as row (row.id)}
      {@render entry(row, 'Scheduled')}
    {:else}
      <p class="muted">No upcoming changes.</p>
    {/each}
    
    {#if !formOpen}
      <button class="add-requirement" type="button" disabled={busy} onclick={() => open(null)}>
        Add requirement
      </button>
    {:else}
      <form class="stack requirement-form" onsubmit={save}>
        <h3>{editing ? `Edit ${editing.isBaseline ? 'initial requirement' : 'requirement'}` : 'Add requirement'}</h3>
        {#if editing?.isBaseline}
          <p class="muted">The initial requirement covers all history until the first change. Its starting month cannot be changed.</p>
        {:else}
          <label>
            Effective from 
            <input type="month" min="0001-02" max="9999-12" required bind:value={month} disabled={busy} />
          </label>
        {/if}
        <label>
          Required office days (%) 
          <input type="number" min="0" max="100" step="1" required bind:value={percentage} disabled={busy} />
        </label>
        <div class="actions">
          <button type="submit" disabled={busy}>{busy ? 'Saving…' : 'Save requirement'}</button>
          <button type="button" class="secondary" disabled={busy} onclick={() => formOpen = false}>Cancel</button>
        </div>
      </form>
    {/if}
    {#if deleting}
      <div class="confirmation" role="group" aria-label="Confirm requirement removal">
        <p>
          Delete the <b>{label(deleting)} requirement ({deleting.requiredOfficePercentage}%)?</b> 
          The preceding percentage will apply until the next change. 
          Calculations for affected months will update.
        </p>
        <div class="actions">
          <button type="button" style="background: var(--danger)" disabled={busy} onclick={remove}>Confirm remove</button>
          <button type="button" class="secondary" disabled={busy} onclick={() => deleting = null}>Cancel remove</button>
        </div>
      </div>
    {/if}
  {/if}
  
  <Feedback {error} {message} />
  
</section>

{#snippet entry(row: Requirement, status: string)}
  <div class="requirement-row">
    <div>
      <strong>{label(row)} · {row.requiredOfficePercentage}%</strong>
      <span class="badge">{status}</span>
      {#if row.isBaseline}
        <p class="muted baseline-note">Applies to all history until the first change.</p>
      {/if}
    </div>
    <div class="actions">
      <button class="secondary" type="button" disabled={busy} aria-label={`Edit ${label(row)}`} onclick={() => open(row)}>Edit</button>
      {#if !row.isBaseline}
        <button class="danger" type="button" disabled={busy} aria-label={`Delete ${label(row)}`} onclick={() => { deleting = row; formOpen = false; error = ''; message = ''; }}>Remove</button>
      {/if}
    </div>
  </div>
{/snippet}

<style>
  .requirement-row { 
    display: flex; 
    justify-content: space-between; 
    align-items: center; 
    gap: 1rem; 
    margin: 1em 0;
    padding: 1em;
    border-radius: 6px;
    background: alpha(from var(--ground) / 20%);
  }
  
  .actions { 
    display: flex; 
    flex-wrap: wrap; 
    gap: .5rem; 
  }
  
  .badge { 
    display: inline-block; 
    margin-left: .5rem; 
    padding: .15rem .5rem; 
    border-radius: 1rem; 
    background: var(--surface-soft, #edf2ee); 
    font-size: .85rem; 
    color: var(--on-green);
  }
  
  .add-requirement { 
    margin-top: 1rem; 
  }
  
  .baseline-note { 
    margin: .4rem 0 0; 
  }
  
  .requirement-form, .confirmation { 
    margin-top: 1rem; max-width: 44rem; 
  }
  
  h3 { 
    margin-top: 1.5rem; 
  }
  @media (max-width: 650px) { 
    .requirement-row { 
      align-items: start; 
      flex-direction: column; 
    }
  }
</style>
