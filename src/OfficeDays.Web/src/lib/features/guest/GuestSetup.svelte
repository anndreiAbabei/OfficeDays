<script lang="ts">
  import { onMount } from 'svelte';
  import { request, errorMessage } from '$lib/api';
  import { isoDate, localDate } from '$lib/dates';
  import type { Jurisdiction } from '$lib/types';
  import type { AttendanceRequirementMode } from '$lib/attendanceRequirementMode';
  import RequirementInput from '$lib/components/RequirementInput.svelte';
  import Feedback from '$lib/components/Feedback.svelte';
  import { readGuestData, saveGuestData, validRequirement, type GuestData } from './storage';
  import { downloadHolidays } from './holidays';
  let { oncomplete }: { oncomplete: (data: GuestData) => void } = $props();
  let calendars = $state<Jurisdiction[]>([]);
  let countryCode = $state('');
  let timeZoneId = $state(Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC');
  let requirementMode = $state<AttendanceRequirementMode>('Percentage');
  let requirementValue = $state<number | undefined>(50);
  let busy = $state(false);
  let loading = $state(true);
  let error = $state('');
  const zones = ['UTC', ...Intl.supportedValuesOf('timeZone')];
  
  async function load(): Promise<void> {
    loading = true; error = '';
    try { calendars = await request<Jurisdiction[]>('/api/holiday-jurisdictions'); }
    catch (failure) { error = errorMessage(failure); }
    finally { loading = false; }
  }
  
  onMount(() => { void load(); });
  
  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    
    if (busy) 
      return;
    
    busy = true; 
    error = '';
    
    try {
      const existing = readGuestData();
      
      if (existing) { 
        oncomplete(existing); 
        return; 
      }
      
      const country = calendars.find((c): boolean => c.code === countryCode);
      
      if (!country) 
        throw new Error('Choose a holiday calendar.');
      
      if (!validRequirement(requirementMode, requirementValue) || requirementValue === undefined) 
        throw new Error('Choose a valid attendance requirement.');
      
      const now = new Date();
      const date = localDate(timeZoneId, now);
      const [current, next] = await Promise.all([
              downloadHolidays(countryCode, date.year), 
              downloadHolidays(countryCode, date.year + 1)
      ]);
      const data: GuestData = { 
        version: 1,
        id: crypto.randomUUID(), 
        createdAt: now.toISOString(), 
        registrationMonth: isoDate(date.year, date.month),
        countryCode, 
        countryName: country.name,
        timeZoneId, 
        teamDay: null,
        attendance: [], 
        plans: [], 
        vacations: [],
        requirements: [{ id: crypto.randomUUID(), createdAt: now.toISOString(), startingFrom: '0001-01-01', requirementMode, requirementValue }],
        holidays: { [date.year]: current, [date.year + 1]: next } };
      const savedDuringDownload = readGuestData();
      
      if (savedDuringDownload) { 
        oncomplete(savedDuringDownload); 
        return; 
      }
      
      saveGuestData(data);
      oncomplete(data);
    } 
    catch (failure) { 
      error = errorMessage(failure); 
    }
    finally { 
      busy = false;
    }
  }
</script>

<section class="card">
  <h1>Continue as guest</h1>
  <p>Your attendance, plans, vacations, and settings stay in this browser. 
    Clearing site data removes them. Download a backup to keep a copy.
  </p>
  <p>Setup downloads your holiday calendars. 
    Afterward, personal tracking works without an internet connection while the app remains open. 
    Holidays are fetched again only when you request them.
  </p>
  {#if loading}
    <p role="status">Loading holiday calendars…</p>
  {:else if !calendars.length}
    <p>No holiday calendars are available.</p>
    <button onclick={load}>Retry calendars</button>
  {:else}
    <form class="stack" onsubmit={submit}>
      <label>
        Country / jurisdiction
        <select required bind:value={countryCode} disabled={busy}>
          <option value="">Select a holiday calendar</option>
          {#each calendars as country}
            <option value={country.code}>{country.name} ({country.code})</option>
          {/each}
        </select>
      </label>
      <label>
        Timezone
        <input list="guest-timezones" required bind:value={timeZoneId} disabled={busy} />
      </label>
      <datalist id="guest-timezones">
        {#each zones as zone}
          <option value={zone}></option>
        {/each}
      </datalist>
      
      <RequirementInput bind:requirementMode bind:requirementValue disabled={busy} />
      <button disabled={busy}>{busy ? 'Downloading holidays and saving setup…' : 'Start tracking'}</button>
    </form>
  {/if}
  
  <Feedback {error} />
  
  <a href="/login">Sign in instead</a>
</section>
