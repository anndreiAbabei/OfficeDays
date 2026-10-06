<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/state';
  import { mutate, errorMessage } from '$lib/api';
  import { signedIn, user } from '$lib/session';
  import type { User } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  
  let username = $state(''); 
  let password = $state(''); 
  let busy = $state(false); 
  let error = $state('');
  
  $effect(() => { if ($user) void goto('/', { replaceState: true }); });
  
  async function submit(event: SubmitEvent) {
    event.preventDefault(); 
    busy = true; 
    error = '';
    
    try { 
      signedIn(await mutate<User>('/api/auth/login', 'POST', { username, password })); 
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
<section class="login-card">
  <div class="brand-mark">OD</div>
  
  <h1>Office Days Tracker</h1>
  
  <p class="muted">Sign in to see your hybrid-working status.</p>
  
  <form class="stack" onsubmit={submit}>
    <label>Username<input autocomplete="username" required bind:value={username} /></label>
    <label>Password<input type="password" autocomplete="current-password" required bind:value={password} /></label>
    <button disabled={busy} type="submit">{busy ? 'Signing in…' : 'Sign in'}</button>
  </form>
  
  <Feedback {error} message={page.url.searchParams.has('registered') ? 'Account created. Sign in to continue.' : ''} />
  <p>New here? <a href="/register">Create an account</a></p>
</section>
