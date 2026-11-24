<script lang="ts">
  import { onMount } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import Feedback from '$lib/components/Feedback.svelte';
  import CopyField from './CopyField.svelte';
  import GuideCarousel from './GuideCarousel.svelte';

  let shortcutUrl = $state<string | null>(null);
  let loading = $state(true);
  let loadError = $state('');
  let createError = $state('');
  let name = $state('');
  let address = $state('');
  let header = $state('');
  let busy = $state(false);

  onMount((): void => {
    address = window.location.origin;
    void load();
  });

  async function load(): Promise<void> {
    loadError = '';
    loading = true;
    try {
      const config = await request<{ shortcutUrl: string | null }>('/api/automation/ios');
      shortcutUrl = config.shortcutUrl;
    } catch (failure) {
      loadError = errorMessage(failure);
    } finally {
      loading = false;
    }
  }

  async function start(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    
    if (!name.trim() || !shortcutUrl || busy || header) 
      return;

    busy = true;
    createError = '';
    
    try {
      const result = await mutate<{ token: string }>('/api/tokens', 'POST', { name: name.trim(), purpose: 'ios-shortcuts' });
      header = 'Bearer ' + result.token;
    } catch (failure) {
      createError = errorMessage(failure);
    } finally {
      busy = false;
    }
  }
</script>

<a href="/automation">← Automation</a>
<h1><img src="/automation/shortcuts.webp" alt="" width="32" height="32" /> Automate with Shortcuts</h1>

<section class="card intro">
  {#if !header}
    <form class="inline-form" onsubmit={start}>
      <label>Device name<input bind:value={name} required maxlength="100" placeholder="e.g. My iPhone" disabled={busy} /></label>
      <button type="submit" disabled={busy || !shortcutUrl || !name.trim()}>{busy ? 'Creating token…' : 'Start'}</button>
    </form>
    <p>Use the built-in Shortcuts app on your iPhone or iPad. Start creates a token for this device and opens the setup guide.</p>
    
    {#if loading}
      <p role="status">Loading shortcut…</p>
    {:else if loadError}
      <Feedback error={loadError} />
      <button type="button" class="secondary" onclick={load}>Retry</button>
    {:else if !shortcutUrl}
      <p role="status">Shortcut setup is unavailable. Ask your administrator to configure the iCloud shortcut link.</p>
    {/if}
    
    <Feedback error={createError} />
  {/if}
  
  <p class="muted">Your device checks when you arrive at the office. OfficeDays receives an attendance request. Your office location stays on your device. <a href="https://github.com/anndreiAbabei/OfficeDays#ios-shortcut-setup" target="_blank" rel="noreferrer">Learn more about OfficeDays and Shortcuts</a>.</p>
</section>

<div class="panels" hidden={!header}>
  {#if header && shortcutUrl}
    <section class="card credentials">
      <h2>Connect {name.trim()}</h2>
      <p class="muted">Keep this page open during setup. You can copy the token only while it’s visible here.</p>
      <CopyField label="OfficeDays address" value={address} action="Copy address" />
      <CopyField label="Authorization header" value={header} action="Copy token" />
      
      <a class="add" href={shortcutUrl} target="_blank" rel="noreferrer">
        <img src="/automation/shortcuts.webp" alt="" width="22" height="22" /> Add to Shortcuts
      </a>
      <p class="muted">Use the address above for this OfficeDays account. If the shortcut has a preset address, verify it in the shortcut editor before the first run.</p>
    
    </section>
  {/if}
  
  <GuideCarousel />
</div>

<style>
  h1 { 
    display: flex; 
    align-items: center; 
    gap: .55rem; 
    margin: .65rem 0 1rem; 
  }
  
  img { 
    border-radius: .35rem; 
  }
  
  form { 
    align-items: end; 
    max-width: 32rem; 
    margin-bottom: .75rem; 
  }
  
  .intro p { 
    margin-bottom: .45rem; 
  }
  
  .intro p:last-child { 
    margin-bottom: 0; 
  }
  
  .panels { 
    display: grid; 
    align-items: start; 
    grid-template-columns: repeat(2, minmax(0, 1fr)); 
    gap: 1rem; 
    margin-top: 1rem; 
  }
  
  .panels[hidden] { 
    display: none; 
  }
  
  .add { 
    display: inline-flex; 
    align-items: center; 
    gap: .55rem; 
    background: var(--green); 
    color: var(--on-green);
    border-radius: .55rem; 
    padding: .65rem .9rem; 
    margin: 1rem 0 .5rem; 
    font-weight: 650; 
    text-decoration: none; 
  }
  
  @media (max-width: 760px) { 
    .panels { 
      grid-template-columns: minmax(0, 1fr); 
    } 
  }
</style>
