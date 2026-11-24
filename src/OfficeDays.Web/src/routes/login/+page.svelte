<script lang="ts">
  import { configuration } from '$lib/features/privacy/configuration';
  import { formatLifetime } from '$lib/features/privacy/formatLifetime';
  import AuthCard from '$lib/components/AuthCard.svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { mutate, errorMessage } from '$lib/api';
  import { signedIn, user, sessionMessage } from '$lib/session';
  import type { User } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  
  let username = $state(''); 
  let password = $state(''); 
  let rememberMe = $state(false);
  let busy = $state(false); 
  let error = $state('');
  
  $effect(() => { if ($user) void goto('/', { replaceState: true }); });
  
  async function submit(event: SubmitEvent): Promise<void> {
    event.preventDefault(); 
    busy = true; 
    error = '';
    
    try { 
      signedIn(await mutate<User>('/api/auth/login', 'POST', { username, password, rememberMe }));
      
      password = ''; 
      
      await goto('/'); 
    }
    catch (failure) { 
      error = errorMessage(failure); 
    }
    finally { 
      busy = false; 
    }
  }
</script>

<svelte:head><title>Sign in · Office Days</title></svelte:head>

<AuthCard>
  
  <h1>Office Days Tracker</h1>
  
  <p class="muted">Sign in to see your hybrid-working status.</p>
  
  <form class="stack" onsubmit={submit}>
    <label>
      Username
      <input autocomplete="username" required bind:value={username} />
    </label>
    <label>
      Password
      <input type="password" autocomplete="current-password" required bind:value={password} />
    </label>
    <label class="remember">
      <input type="checkbox" bind:checked={rememberMe} />
      <span>Keep me signed in on this device {#if $configuration.status === 'ready'} ({formatLifetime($configuration.persistentLoginSeconds)}){/if}</span>
    </label>
    <button disabled={busy} type="submit">
      {busy ? 'Signing in…' : 'Sign in'}
    </button>
  </form>
  
  <Feedback {error} message={$sessionMessage || (page.url.searchParams.has('registered') ? 'Account created. Sign in to continue.' : '')} />
  
  <p class="register-link">
    New here? 
    <a href="/register">Create an account</a>
  </p>
</AuthCard>

<style>
  .remember { 
    display: flex; 
    align-items: center; 
    gap: .6rem; 
    cursor: pointer; 
    min-height: 44px; 
  }
  
  .remember input { 
    width: 1.1rem; 
    height: 1.1rem; 
    min-height: 0; 
  }
  
  .register-link {
    margin-top: 2rem;
  }
</style>