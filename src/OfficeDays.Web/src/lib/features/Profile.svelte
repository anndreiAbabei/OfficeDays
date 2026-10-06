<script lang="ts">
  import { untrack } from 'svelte';
  import { mutate, errorMessage } from '$lib/api';
  import type { User } from '$lib/types';
  import Feedback from '$lib/components/Feedback.svelte';
  let { currentUser, onsave }: { currentUser: User; onsave: (user: User) => void } = $props();
  let email = $state(untrack(() => currentUser.email || '')); let percentage = $state(untrack(() => currentUser.requiredOfficePercentage));
  let busy = $state(false); let error = $state(''); let message = $state('');
  async function save(event: SubmitEvent) {
    event.preventDefault(); busy = true; error = ''; message = '';
    try {
      const updated = await mutate<User>('/api/users/me', 'PUT', { email: email.trim() || null, requiredOfficePercentage: percentage });
      email = updated.email || ''; percentage = updated.requiredOfficePercentage; onsave(updated); message = 'Profile saved.';
    } catch (failure) { error = errorMessage(failure); }
    finally { busy = false; }
  }
</script>
<section class="card">
  <h2>Your preferences</h2><p class="muted">Your email address is optional.</p>
  <form class="profile-form stack" onsubmit={save}>
    <label>Email address<input type="email" autocomplete="email" maxlength="254" bind:value={email} /></label>
    <label>Required office days (%)<input type="number" min="0" max="100" step="1" required bind:value={percentage} /></label>
    <button type="submit" disabled={busy}>{busy ? 'Saving…' : 'Save profile'}</button>
  </form>
  <Feedback {error} {message} />
</section>

<style>
  .profile-form { max-width: 44rem; align-items: end; }
  @media (min-width: 651px) {
    .profile-form { grid-template-columns: minmax(0, 2fr) minmax(0, 1fr) auto; }
  }
</style>
