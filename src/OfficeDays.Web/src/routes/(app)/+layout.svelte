<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { user, sessionReady, signedOut } from '$lib/session';
  import { mutate, errorMessage } from '$lib/api';
  import Feedback from '$lib/components/Feedback.svelte';
  import type { Snippet } from 'svelte';
  import type { User } from "$lib/types";
  
  let { children }: { children: Snippet } = $props();
  let busy = $state(false);
  let error = $state('');
  function getPages(user: User): { href: string, label: string }[] {
    return [
      { href: '/',                label: 'Dashboard' },
      { href: '/profile',         label: 'Profile' },
      { href: '/reports',         label: 'Reports' },
      { href: '/tokens',          label: 'API tokens' },
      ...(user.isAdmin 
              ? [{href: '/admin', label: 'Administration'}] 
              : [])
    ];
  }
  
  $effect(() => { if ($sessionReady && !$user) void goto('/login', { replaceState: true }); });
  
  async function logout(): Promise<void> {
    busy = true; 
    error = '';
    
    try { 
      await mutate('/api/auth/logout', 'POST'); 
      
      signedOut(); 
      
      await goto('/login'); 
    } catch (failure) {
      error = errorMessage(failure); 
    }
    finally { 
      busy = false; 
    }
  }
</script>
{#if $user}
  <header class="topbar">
    <div>
      <a href="/" class="eyebrow">Office Days Tracker</a>
      <p class="account-detail">
        {$user.username} · {$user.countryName} ({$user.countryCode}) · {$user.timeZoneId}
      </p>
    </div>
    <button class="secondary small" disabled={busy} onclick={logout}>
      Sign out
    </button>
  </header>
  
  <div class="app-layout">
    <nav class="app-nav" aria-label="Main navigation">
      {#each getPages($user) as link}
        <a href={link.href} aria-current={page.url.pathname === link.href ? 'page' : undefined}>{link.label}</a>
      {/each}
    </nav>
    <div class="page-content">
      <Feedback {error} />{@render children()}
    </div>
  </div>
{/if}

<style>
  .topbar { 
    display: flex; 
    align-items: center; 
    gap: .7rem; 
  }
  
  .topbar { 
    justify-content: space-between; 
  }
  
  .topbar { 
    margin-bottom: 1.25rem; 
  }
  
  .eyebrow {
    margin-bottom: .1rem; 
    color: var(--green); 
    font-size: .72rem; 
    font-weight: 800; 
    letter-spacing: .11em; 
    text-transform: uppercase; 
    text-decoration: none;
  }

  .eyebrow:hover {
    color: var(--green-dark);
  }

  @media (max-width: 650px) {
    .topbar { align-items: start; }
  }
  
  .page-content { 
    min-width: 0; 
  }
  
  .page-content > :global(h1) { 
    margin-bottom: 1rem; 
  }
  
  .account-detail { 
    margin: .25rem 0 0; 
    font-size: .85rem; 
    color: var(--muted); 
    overflow-wrap: anywhere; 
  }
  
  .topbar > div { 
    min-width: 0; 
  }
  .topbar > button { 
    flex-shrink: 0; 
  }
  
  .app-layout { 
    display: grid; 
    gap: 1rem; 
  }
  
  .app-nav { 
    display: flex; 
    flex-wrap: wrap; 
    gap: .4rem; 
    align-content: start;
  }
  
  .app-nav a { 
    display: flex; 
    align-items: center; 
    min-height: 44px; 
    padding: .6rem .8rem; 
    border-radius: .55rem; 
    text-decoration: none; 
    color: var(--muted); 
    font-weight: 650; 
  }
  
  .app-nav a:hover { 
    background: var(--secondary); 
  }
  
  .app-nav a[aria-current="page"] { 
    color: var(--green-dark); 
    background: var(--green-soft); 
  }
  
  @media (min-width: 1050px) {
    .app-layout { grid-template-columns: 180px minmax(0, 1fr); gap: 1.75rem; }
    .app-nav { flex-direction: column; position: sticky; top: 1rem; align-self: start; }
  }
  @media (max-width: 650px) {
    .app-nav { gap: .2rem; }
    .app-nav a { padding: .5rem .65rem; font-size: .9rem; }
  }
</style>
