<script lang="ts">
  import '../app.css';
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { request } from '$lib/api';
  import { loadSession, sessionReady, sessionError } from '$lib/session';
  import type { Snippet } from 'svelte';
  import type { AppVersion } from "$lib/types";
  
  let { children }: { children: Snippet } = $props();
  let version = $state('Loading version…');
  let author = $state('Andrei');
  let gitHubUrl = $state('#');
  
  onMount(() => {
    // Preserve links from the original UI.
    void (async () => {
      if (location.hash === '#register' || location.hash === '#login')
        await goto(`/${location.hash.slice(1)}`, { replaceState: true });
      
      await loadSession();
    })();
    void request<AppVersion>('/api/version')
            .then(data => {
              version = `v${data.version}`;
              author = data.author;
              gitHubUrl = data.gitHubRepo
            })
            .catch(() => version = 'Version unavailable');
  });
</script>
<a class="skip-link" href="#main-content">
  Skip to content
</a>

<main id="main-content" class="shell" tabindex="-1">
  {#if $sessionError}
    <section class="card">
      <p class="error" role="alert">{$sessionError}</p>
      <button onclick={loadSession}>
        Retry connection
      </button>
    </section>
  {:else if $sessionReady || page.url.pathname === '/register' || page.url.pathname === '/login'}
    {@render children()}
  {:else}
    <p class="muted" role="status">Loading Office Days…</p>
  {/if}
</main>
<footer class="app-footer muted">
  <span>Created by <a href="{gitHubUrl}">{author}</a> · </span>
  <span>Office Days · {version}</span>
</footer>

<style>
  .shell { 
    width: min(1280px, calc(100% - 2rem)); 
    margin: 2rem auto 5rem; 
  }
  
  .app-footer { 
    text-align: center; 
    padding: 1rem 1rem 2rem; 
    font-size: .8rem; 
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
