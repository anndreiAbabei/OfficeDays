<script lang="ts">
  import { untrack } from 'svelte';
  import { errorMessage } from '$lib/api';
  import { localDate, months } from '$lib/dates';
  import type { Appearance, Period, TeamDay } from '$lib/types';
  import PreferencesForm from '../profile/PreferencesForm.svelte';
  import PeriodPicker from '$lib/components/PeriodPicker.svelte';
  import Feedback from '$lib/components/Feedback.svelte';
  import Dashboard from '../Dashboard.svelte';
  import Reports from '../Reports.svelte';
  import AttendanceRequirements from '../AttendanceRequirements.svelte';
  import { provideDataSource } from '../dataSource';
  import { createGuestSource } from './dataSource';
  import { readGuestData, saveGuestData, guestStorageKey, type GuestData } from './storage';
  import GuestSetup from './GuestSetup.svelte';
  import { downloadHolidays } from './holidays';
  import { downloadGuestData } from './export';
  
  let { initial }: { initial: GuestData | null } = $props();
  let data = $state(untrack((): GuestData | null => initial));
  let view = $state<'dashboard' | 'reports' | 'profile'>('dashboard');
  let period = $state<Period>(untrack((): Period => localDate(initial?.timeZoneId ?? 'UTC')));
  let reportYear = $state(untrack((): number => period.year));
  let revision = $state(0);
  let busy = $state(false);
  let error = $state('');
  let profileMessage = $state('');
  let teamDay = $derived<TeamDay | null>(data?.teamDay ?? null);
  let appearance = $derived<Appearance>(data?.appearance ?? 'System');

  $effect(() => {
    const root = document.documentElement;
    root.dataset.guestAppearance = data?.appearance ?? 'System';
    
    return (): void => { delete root.dataset.guestAppearance; };
  });
  
  provideDataSource(createGuestSource());
  
  const targetYear = $derived(view === 'reports' ? reportYear : period.year);
  const missing = $derived(data !== null && !Object.hasOwn(data.holidays, String(targetYear).padStart(4, '0')));
  
  function complete(value: GuestData): void {
    data = value; 
    period = localDate(value.timeZoneId); 
    reportYear = period.year; 
  }
  
  function currentData(): GuestData {
    const current = readGuestData();
    
    if (!current) 
      throw new Error('Guest data is no longer available in this browser.');
    
    return current;
  }
  async function holidays(): Promise<void> {
    if (!data || busy) 
      return;
    
    busy = true;
    error = '';
    
    const year = targetYear;
    
    try {
      const downloaded = await downloadHolidays(data.countryCode, year);
      const current = currentData();
      current.holidays[String(year).padStart(4, '0')] = downloaded;
      saveGuestData(current); data = current; revision++;
    } 
    catch (failure) {
      error = `Unable to get holidays. Connect to the internet and try again. ${errorMessage(failure)}`; 
    }
    finally {
      busy = false; 
    }
  }
  
  function backup(): void {
    error = '';
    try { 
      downloadGuestData(currentData()); 
    } 
    catch (failure) { 
      error = errorMessage(failure); 
    }
  }
  
  function savePreferences(): void {
    profileMessage = '';
    error = '';
    try {
      const current = currentData();
      current.teamDay = teamDay;
      current.appearance = appearance;
      saveGuestData(current);
      data = current;
      profileMessage = 'Profile saved.';
    } catch (failure) {
      error = errorMessage(failure);
    }
  }

  function storageChanged(event: StorageEvent): void {
    // Re-render other tabs after an edit; never replace their saved data with a stale snapshot.
    if (event.storageArea !== localStorage || (event.key !== null && event.key !== guestStorageKey)) 
      return;
    
    try { 
      data = readGuestData(); 
      revision++;
    } 
    catch (failure) {
      error = errorMessage(failure); 
    }
  }
</script>

<svelte:window onstorage={storageChanged} />

{#if !data}
  <GuestSetup oncomplete={complete} />
{:else}
  <header class="guest-header">
    <div>
      <h1>Office Days · Guest</h1>
      <span class="muted">Saved in this browser</span>
    </div>
    <a href="/login">Sign in</a>
  </header>
  <nav aria-label="Guest navigation">
    <button class="secondary" aria-current={view === 'dashboard' ? 'page' : undefined} onclick={(): void => { view = 'dashboard'; }}>
      Dashboard
    </button>
    <button class="secondary" aria-current={view === 'profile' ? 'page' : undefined} onclick={(): void => { view = 'profile'; profileMessage = ''; }}>
      Profile
    </button>
    <button class="secondary" aria-current={view === 'reports' ? 'page' : undefined} onclick={(): void => { view = 'reports'; }}>
      Reports
    </button>
  </nav>
  
  <Feedback {error} />
  
  {#if view !== 'profile' && missing}
    <section class="card" role="status">
      <p>
        Holidays for {targetYear} have not been downloaded. 
        Calculated targets may be incomplete. 
        You can continue tracking without them.
      </p>
      <p class="muted">
        An internet connection is needed to get this year's calendar from the server.
      </p>
      <button onclick={holidays} disabled={busy}>{busy ? 'Getting holidays…' : 'Get holidays'}</button>
    </section>
  {/if}
  {#if view === 'dashboard'}
    <h2>{months[period.month - 1]} {period.year}</h2>
    <PeriodPicker {period} onchange={(value): void => { period = value; error = ''; }} />
    {#if `${String(period.year).padStart(4, '0')}-${String(period.month).padStart(2, '0')}-01` < data.registrationMonth}
      <p role="status">Your tracking history starts in {data.registrationMonth.slice(0, 7)}.</p>
    {/if}
    {#key `${period.year}-${period.month}-${revision}`}
      <Dashboard {period} timeZone={data.timeZoneId} teamDay={data.teamDay} />
    {/key}
  {:else if view === 'reports'}
    {#key revision}
      <Reports timeZone={data.timeZoneId} initialYear={reportYear} onyearselection={(year): void => { reportYear = year; }} />
    {/key}
  {:else}
    <section class="card" onchange={(): void => { profileMessage = ''; }}>
      <h2>Guest profile</h2>
      <p>{data.countryName} · {data.timeZoneId}</p>
      <PreferencesForm bind:teamDay bind:appearance saveLabel="Save" onsave={savePreferences} />
      <Feedback message={profileMessage} />
    </section>
    
    <AttendanceRequirements onsave={(): void => { data = currentData(); }} />
    <section class="card">
      <h2>Download your data</h2>
      <p>Download a JSON backup of your tracking data and holiday calendars before clearing browser data.</p>
      <button onclick={backup}>Download guest data</button>
    </section>
  {/if}
{/if}

<style>
  .guest-header { display: flex; align-items: center; justify-content: space-between; gap: 1rem; }
  .guest-header h1 { font-size: 1.15rem; margin: 0; }

  nav { 
    display: flex; 
    flex-wrap: wrap; 
    gap: .5rem; 
    margin: 1rem 0; 
  }
  
  [aria-current='page'] { 
    outline: 2px solid var(--green); 
  }
  
  .card { 
    margin: 1rem 0; 
  }
</style>
