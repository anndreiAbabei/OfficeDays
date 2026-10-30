<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import type { Component } from 'svelte';
  import { user } from '$lib/session';
  import { errorMessage } from '$lib/api';
  import { readGuestData, type GuestData } from '$lib/features/guest/storage';
  
  let GuestApp = $state<Component<{ initial: GuestData | null }> | null>(null);
  let initial = $state<GuestData | null>(null);
  let error = $state('');
  let loading = $state(true);
  
  async function load(): Promise<void> {
    if ($user) { 
      await goto('/', { replaceState: true }); 
      return; 
    }
    
    loading = true; 
    error = '';
    
    try {
      initial = readGuestData();
      GuestApp = (await import('$lib/features/guest/GuestApp.svelte')).default;
    } 
    catch (failure) { 
      error = errorMessage(failure); 
    }
    finally {
      loading = false; 
    }
  }
  onMount(() => { void load(); });
</script>

<svelte:head><title>Guest · Office Days</title></svelte:head>

{#if loading}
  <section class="card" role="status">
    <p>Loading guest features…</p>
    <progress aria-label="Loading guest features"></progress>
  </section>
{:else if error}
  <section class="card">
    <p role="alert">{error}</p>
    <button onclick={load}>Retry guest mode</button>
    <a href="/login">Sign in</a>
  </section>
{:else if GuestApp}
  <GuestApp {initial} />
{/if}
