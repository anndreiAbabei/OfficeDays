<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { user, sessionReady, signedOut } from '$lib/session';
  import { mutate, errorMessage } from '$lib/api';
  import Feedback from '$lib/components/Feedback.svelte';
  import type { Snippet } from 'svelte';
  let { children }: { children: Snippet } = $props();
  let busy = $state(false);
  let error = $state('');
  $effect(() => { if ($sessionReady && !$user) void goto('/login', { replaceState: true }); });
  async function logout() {
    busy = true; error = '';
    try { await mutate('/api/auth/logout', 'POST'); signedOut(); await goto('/login'); }
    catch (failure) { error = errorMessage(failure); }
    finally { busy = false; }
  }
</script>
{#if $user}
  <header class="topbar">
    <div><p class="eyebrow">Office Days Tracker</p><p class="account-detail">{$user.username} · {$user.countryName} ({$user.countryCode}) · {$user.timeZoneId}</p></div>
    <button class="secondary small" disabled={busy} onclick={logout}>Sign out</button>
  </header>
  <div class="app-layout">
    <nav class="app-nav" aria-label="Main navigation">
      {#each [{ href: '/', label: 'Dashboard' }, { href: '/profile', label: 'Profile' }, { href: '/tokens', label: 'API tokens' }, ...($user.isAdmin ? [{ href: '/admin', label: 'Administration' }] : [])] as link}
        <a href={link.href} aria-current={page.url.pathname === link.href ? 'page' : undefined}>{link.label}</a>
      {/each}
    </nav>
    <div class="page-content"><Feedback {error} />{@render children()}</div>
  </div>
{/if}
