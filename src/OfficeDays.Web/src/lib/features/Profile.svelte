<script lang="ts">
  import { untrack } from 'svelte';
  import { mutate, errorMessage } from '$lib/api';
  import type { TeamDay, User } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  
  let { currentUser, onsave }: { currentUser: User; onsave: (user: User) => void } = $props();
  let email = $state(untrack(() => currentUser.email || ''));
  const teamDays: TeamDay[] = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday'];
  let teamDay = $state<TeamDay | null>(untrack(() => currentUser.teamDay));
  let busy = $state(false); 
  let error = $state(''); 
  let message = $state('');
  
  async function save(event: SubmitEvent): Promise<void> {
    event.preventDefault(); 
    busy = true; 
    error = ''; 
    message = '';
    
    try {
      const updated = await mutate<User>('/api/users/me', 'PUT', { email: email.trim() || null, teamDay });
      
      email = updated.email || ''; 
      teamDay = updated.teamDay; 
      onsave(updated); 
      
      message = 'Profile saved.';
    } catch (failure) { 
      error = errorMessage(failure); 
    }
    finally { 
      busy = false;
    }
  }
</script>

<section class="card">
  <h2>Your preferences</h2><p class="muted">Your email address is optional.</p>
  <form class="profile-form stack" onsubmit={save}>
    <label>
      Email address
      <input type="email" autocomplete="email" maxlength="254" bind:value={email} />
    </label>
    <label>
      Team day
      <select bind:value={teamDay} aria-describedby="team-day-help">
        <option value={null}>None</option>
        {#each teamDays as day}
          <option value={day}>{day[0].toUpperCase() + day.slice(1)}</option>
        {/each}
      </select>
      <span id="team-day-help" class="muted">Your team's weekly office day. Highlighted in your calendar.</span>
    </label>
    <button type="submit" disabled={busy}>
      {busy ? 'Saving…' : 'Save profile'}
    </button>
  </form>
  
  <Feedback {error} {message} />
</section>

<style>
  .profile-form { 
    max-width: 44rem; 
    align-items: start; 
  }
  
  .profile-form button {
    grid-column: 2;
  }
  
  #team-day-help {
    font-weight: initial;
  }
  
  @media (min-width: 651px) {
    .profile-form { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  }
</style>
