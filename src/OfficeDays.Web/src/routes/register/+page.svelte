<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { errorMessage, mutate, request } from '$lib/api';
  import type { Jurisdiction } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  let username = $state(''); 
  let email = $state(''); 
  let password = $state(''); 
  let confirmation = $state('');
  let timeZoneId = $state(Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC');
  let countryCode = $state(''); let requiredOfficePercentage = $state(50);
  let calendars = $state<Jurisdiction[]>([]); 
  let loading = $state(true); 
  let busy = $state(false); 
  let error = $state(''); 
  let calendarError = $state('');
  
  const zones = typeof Intl.supportedValuesOf === 'function' ? Intl.supportedValuesOf('timeZone') : ['UTC'];
  
  async function loadCalendars() {
    loading = true; calendarError = '';
    try {
      calendars = await request<Jurisdiction[]>('/api/holiday-jurisdictions');
      if (calendars.length === 1) countryCode = calendars[0].code;
      if (!calendars.length) calendarError = 'No holiday calendars are available. Please contact the administrator.';
    } catch (failure) { calendarError = errorMessage(failure); }
    finally { loading = false; }
  }
  
  onMount(() => { void loadCalendars(); });
  
  async function submit(event: SubmitEvent) {
    event.preventDefault();
    error = '';
    
    if (password !== confirmation) {
      error = 'Passwords do not match.';
      return; 
    }
    
    busy = true;
    try {
      await mutate('/api/users', 'POST', { username: username.trim(), 
                                           email: email.trim() || null, 
                                           password, 
                                           timeZoneId: timeZoneId.trim(), 
                                           countryCode, 
                                           requiredOfficePercentage 
                                         });
      password = ''; 
      confirmation = ''; 
      await goto('/login?registered=1');
    } catch (failure) {
      error = errorMessage(failure); 
    }
    finally { 
      busy = false; 
    }
  }
</script>
<svelte:head><title>Create an account · Office Days</title></svelte:head>
<section class="login-card register-card">
  <div class="brand-mark">OD</div>
  
  <h1>Create an account</h1>
  <p class="muted">Start tracking your office days.</p>
  
  <form class="stack" onsubmit={submit}>
    <label>Username<input autocomplete="username" minlength="3" maxlength="100" required bind:value={username} /></label>
    <label>Email address (optional)<input type="email" autocomplete="email" maxlength="254" bind:value={email} /></label>
    <label>Password<input type="password" autocomplete="new-password" minlength="12" required bind:value={password} aria-describedby="password-help" /></label>
    <p id="password-help" class="muted">Use at least 12 characters.</p>
    <label>Confirm password<input type="password" autocomplete="new-password" minlength="12" required bind:value={confirmation} /></label>
    <label>Timezone<input list="timezone-options" required bind:value={timeZoneId} /></label>
    <datalist id="timezone-options">{#each zones as zone}<option value={zone}></option>{/each}</datalist>
    <label>Holiday calendar<select required bind:value={countryCode}><option value="">Select a holiday calendar</option>{#each calendars as item}<option value={item.code}>{item.name} ({item.code})</option>{/each}</select></label>
    <label>Required office days (%)<input type="number" min="0" max="100" step="1" required bind:value={requiredOfficePercentage} /></label>
    <button type="submit" disabled={busy || loading || !!calendarError}>{busy ? 'Creating account…' : 'Create account'}</button>
  </form>
  <Feedback error={error || calendarError} />
  {#if calendarError}<button class="secondary" onclick={loadCalendars} disabled={loading}>Retry loading calendars</button>{/if}
  <p>Already have an account? <a href="/login">Sign in</a></p>
</section>
