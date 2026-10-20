<script lang="ts">
  import { mutate, errorMessage } from '$lib/api';
  import { signedOut, sessionMessage } from '$lib/session';
  import Feedback from '$lib/components/Feedback.svelte';

  let dialog: HTMLDialogElement;
  let trigger: HTMLButtonElement;
  let password = $state('');
  let busy = $state(false);
  let error = $state('');

  function open(): void {
    password = '';
    error = '';
    
    dialog.showModal();
  }

  function close(): void {
    if (busy) 
      return;
    
    password = ''; 
    error = '';
    
    dialog.close();
    trigger.focus();
  }

  function cancel(event: Event): void {
    event.preventDefault();
    close();
  }

  async function remove(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    
    if (busy) 
      return;
    
    busy = true; 
    error = '';
    
    try {
      await mutate<void>('/api/users/me', 'DELETE', { password });
      
      password = '';
      
      dialog.close();
      sessionMessage.set('Your account and personal data have been deleted.');
      signedOut();
    } catch (failure) {
      error = errorMessage(failure);
    } finally {
      password = '';
      busy = false;
    }
  }
</script>

<section class="card danger-section" aria-labelledby="danger-heading">
  <h2 id="danger-heading">Danger section</h2>
  <p>
    Deleting your account permanently removes your profile, preferences, attendance, plans, vacations, requirement history, and API tokens. <b>This cannot be undone.</b>
  </p>
  <p class="muted">
    Download or save a copy of your data before deleting your account. Shared holiday calendars and other users’ data are kept.
  </p>
  <button bind:this={trigger} type="button" class="danger" onclick={open}>Delete your account</button>
</section>

<dialog bind:this={dialog} oncancel={cancel} aria-labelledby="delete-account-heading" aria-describedby="delete-account-warning">
  <h2 id="delete-account-heading">
    Delete your account?
  </h2>
  <p id="delete-account-warning">
    This permanently deletes your personal data and signs you out everywhere. <b>You cannot undo this action.</b>
  </p>
  <form class="stack" onsubmit={remove} aria-busy={busy}>
    <label>Current password
      <input type="password" autocomplete="current-password" required bind:value={password} disabled={busy} />
    </label>
    <Feedback {error} />
    <div class="actions">
      <button type="button" class="secondary" onclick={close} disabled={busy}>Cancel</button>
      <button type="submit" class="danger" disabled={busy || !password}>
        {busy ? 'Deleting account…' : 'Permanently delete account'}
      </button>
    </div>
  </form>
</dialog>

<style>
  .danger-section { 
    border: 1px solid var(--danger); 
  }
  
  dialog { 
    max-width: 32rem; 
    width: calc(100% - 2rem); 
    max-height: calc(100% - 2rem); 
    overflow: auto; 
    padding: 1.5rem; 
    border: 1px solid var(--danger); 
    border-radius: .75rem; 
    color: var(--ink); 
    background: var(--paper); 
  }
  
  dialog::backdrop { 
    background: rgb(0 0 0 / 60%); 
  }
  
  .actions { 
    display: flex; 
    flex-wrap: wrap; 
    gap: .75rem; 
  }
  
  button.danger { 
    border: 1px solid var(--danger); 
    padding: .6rem 1rem; font-weight: 700; 
  }
</style>
