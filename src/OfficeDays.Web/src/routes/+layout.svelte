<script lang="ts">
  import '../app.css';
  import CookieNotice from '$lib/components/CookieNotice.svelte';
  import { configuration, loadConfiguration } from '$lib/features/privacy/configuration';
  import OfflineStatus from '$lib/components/OfflineStatus.svelte';
  import { onMount } from 'svelte';
  import { afterNavigate, goto } from '$app/navigation';
  import { page } from '$app/state';
  import VersionInfo from '$lib/components/VersionInfo.svelte';
  import { readGuestData } from '$lib/features/guest/storage';
  import { loadSession, sessionReady, sessionError } from '$lib/session';
  import type { Snippet } from 'svelte';
  
  let { children }: { children: Snippet } = $props();
  afterNavigate(({ to }): void => {
    if (to?.route.id !== '/guest') delete document.documentElement.dataset.guestAppearance;
  });

  let hasGuestData = $state(false);
  async function openGuest(): Promise<void> {
    sessionError.set('');
    await goto('/guest');
  }
  onMount(() => {
    void loadConfiguration();
    // Preserve links from the original UI.
    void (async () => {
      if (location.hash === '#register' || location.hash === '#login')
        await goto(`/${location.hash.slice(1)}`, { replaceState: true });
      
      await loadSession();
      try { 
        hasGuestData = readGuestData() !== null; 
      } 
      catch { 
        hasGuestData = true; 
      }
    })();
  });
</script>
<a class="skip-link" href="#main-content">
  Skip to content
</a>

<div class="app-frame">
  <main id="main-content" class="shell" tabindex="-1">
    {#if page.route.id === '/privacy'}
      {@render children()}
    {:else if $sessionError}
      <section class="card">
        <p class="error" role="alert">{$sessionError}</p>
        <button onclick={loadSession}>
          Retry connection
        </button>
        {#if hasGuestData}
          <button class="secondary" onclick={openGuest}>Open saved guest data</button>
        {/if}
      </section>
    {:else if $sessionReady || page.url.pathname === '/register' || page.url.pathname === '/login'}
      {@render children()}
    {:else}
      <p class="muted" role="status">Loading Office Days…</p>
    {/if}
  </main>
  <footer class="app-footer muted">
    <OfflineStatus />
    <VersionInfo />
    {#if $configuration.status === 'ready' && $configuration.privacyNotice}
      <a class="privacy-notice" href="/privacy">
        <p>Privacy notice</p>
      </a>
      <CookieNotice persistentLoginSeconds={$configuration.persistentLoginSeconds} />
    {:else if $configuration.status === 'error'}
      <p role="status">Unable to load the privacy settings for this instance.</p>
      <button class="small secondary" onclick={loadConfiguration}>Retry privacy settings</button>
    {/if}
  </footer>
</div>

<style>
  .app-frame {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
    min-height: 100dvh;
  }


  .shell { 
    flex: 1;
    width: min(1280px, calc(100% - 2rem)); 
    margin: 2rem auto 5rem; 
  }
  
  .app-footer { 
    text-align: center; 
    padding: 1rem 1rem 2rem; 
    font-size: .8rem; 
  }
  
  .app-footer .privacy-notice > p {
    margin-top: 1em;
  }
  
  .skip-link:not(:focus) { 
    position: absolute;
    width: 1px; 
    height: 1px; 
    padding: 0; 
    margin: -1px; 
    overflow: hidden; 
    clip-path: inset(50%); 
    white-space: nowrap; 
  }
  
  .skip-link:focus { 
    display: block; 
    padding: 1rem; 
  }
  
  @media (max-width: 650px) {
    .shell { width: calc(100% - 1rem); margin-top: .8rem; }
  }
</style>
