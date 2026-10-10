<script lang="ts">
  import { onMount } from 'svelte';
  import { request, errorMessage } from '$lib/api';
  import { periodQuery } from '$lib/dates';
  import type { Status, Period } from '$lib/types';
  import StatusSummary from '$lib/components/StatusSummary.svelte';
  import Feedback from '$lib/components/Feedback.svelte';
  import PlanningCalendar from './PlanningCalendar.svelte';
  import AttendanceList from './AttendanceList.svelte';
  import VacationList from './VacationList.svelte';
  
  let { period, timeZone }: { period: Period; timeZone: string } = $props();
  let status = $state<Status | null>(null); let loading = $state(true); let error = $state('');
  let revision = 0;
  let calendarRevision = $state(0);
  let plannedCount = $state<number | null>(null);

  function entriesChanged(): void {
    calendarRevision++;
    plannedCount = null;
    void refreshSummary();
  }
  
  async function refreshSummary(): Promise<void> {
    const current = ++revision; 
    loading = true; 
    error = '';
    
    try { 
      const result = await request<Status>(`/api/status?${periodQuery(period)}`); 
      if (current === revision) 
        status = result; 
    } catch (failure) { 
      if (current === revision) 
        error = errorMessage(failure); 
    }
    finally { 
      if (current === revision) 
        loading = false; 
    }
  }
  onMount(() => { void refreshSummary(); });
</script>
<StatusSummary {status} {loading} {plannedCount} />
<Feedback {error} />
{#if error}
  <button class="secondary" onclick={refreshSummary}>Retry summary</button>
{/if}

<PlanningCalendar {period} {timeZone} revision={calendarRevision}
  oncountchange={count => plannedCount = count} onchange={refreshSummary} />

<div class="columns">
  <AttendanceList {period} {timeZone} onchange={entriesChanged} />
  <VacationList {period} onchange={entriesChanged} />
</div>

<style>
  .columns { 
    display: grid; 
    grid-template-columns: 1fr 1fr; 
    gap: 1rem; 
    margin-bottom: 1rem; 
  }
  @media (max-width: 650px) {
    .columns { grid-template-columns: 1fr; }
  }
  .columns > :global(*) { 
    min-width: 0; 
  }
  .columns { 
    align-items: start; 
  }
</style>
