<script lang="ts">
  import { untrack } from 'svelte';
  import { mutate, errorMessage } from '$lib/api';
  import type { Appearance, TeamDay, User } from '$lib/types';
  import PreferencesForm from './PreferencesForm.svelte';
  import Feedback from '$lib/components/Feedback.svelte';
  
  let { currentUser, onsave }: { currentUser: User; onsave: (user: User) => void } = $props();
  let email = $state(untrack(() => currentUser.email || ''));
  let teamDay = $state<TeamDay | null>(untrack(() => currentUser.teamDay));
  let appearance = $state<Appearance>(untrack((): Appearance => currentUser.appearance));
  let busy = $state(false); 
  let error = $state(''); 
  let message = $state('');
  
  async function save(): Promise<void> {
    busy = true; 
    error = ''; 
    message = '';
    
    try {
      const updated = await mutate<User>('/api/users/me', 'PUT', { email: email.trim() || null, teamDay, appearance });
      
      email = updated.email || ''; 
      appearance = updated.appearance;
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
  <h2>Your preferences</h2>
  <PreferencesForm bind:teamDay bind:appearance {busy} saveLabel="Save profile" onsave={save}>
    <label>
      Email address
      <input type="email" autocomplete="email" maxlength="254" placeholder="Optional" bind:value={email} />
    </label>
  </PreferencesForm>
  <Feedback {error} {message} />
</section>
