<script lang="ts">
  import '../app.css';
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { request } from '$lib/api';
  import { loadSession, sessionReady, sessionError } from '$lib/session';
  import type { Snippet } from 'svelte';
  let { children }: { children: Snippet } = $props();
  let version = $state('Loading version…');
  onMount(() => {
    // Preserve links from the original UI.
    void (async () => {
      if (location.hash === '#register' || location.hash === '#login') await goto(`/${location.hash.slice(1)}`, { replaceState: true });
      await loadSession();
    })();
    void request<{ version: string }>('/api/version').then(data => version = `v${data.version}`).catch(() => version = 'Version unavailable');
  });
</script>
<a class="skip-link" href="#main-content">Skip to content</a>
<main id="main-content" class="shell" tabindex="-1">
  {#if $sessionError}
    <section class="card"><p class="error" role="alert">{$sessionError}</p><button onclick={loadSession}>Retry connection</button></section>
  {:else if $sessionReady || page.url.pathname === '/register' || page.url.pathname === '/login'}
    {@render children()}
  {:else}
    <p class="muted" role="status">Loading Office Days…</p>
  {/if}
</main>
<footer class="app-footer muted">Office Days · {version}</footer>
