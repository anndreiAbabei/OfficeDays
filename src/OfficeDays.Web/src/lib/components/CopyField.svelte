<script lang="ts">
  import Feedback from '$lib/components/Feedback.svelte';

  let { label, value, action }: { label: string; value: string; action: string } = $props();
  const inputId = $props.id();
  let input: HTMLInputElement;
  let message = $state('');
  let error = $state('');

  async function copy(): Promise<void> {
    message = '';
    error = '';
    
    try {
      await navigator.clipboard.writeText(value);
      message = label + ' copied.';
    } catch {
      input.focus();
      input.select();
      error = 'Copy the selected text manually.';
    }
  }
</script>

<div class="field">
  <label for={inputId}>{label}</label>
  <div class="control">
    <input id={inputId} readonly {value} bind:this={input} spellcheck="false" />
    <button type="button" aria-label={action} title={action} onclick={copy}>
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <rect x="8" y="8" width="12" height="13" rx="2" />
        <path d="M15 5V4a1 1 0 0 0-1-1H5a2 2 0 0 0-2 2v10a1 1 0 0 0 1 1h1" />
      </svg>
    </button>
  </div>
</div>
<Feedback {message} {error} />

<style>
  .field {
    display: grid;
    gap: .3rem;
    margin-top: .9rem;
  }

  .control {
    position: relative;
    min-width: 0;
  }

  input {
    display: block;
    min-height: 46px;
    padding-inline-end: calc(44px + .5rem);
  }

  button {
    position: absolute;
    inset-block: 1px;
    inset-inline-end: 1px;
    display: grid;
    place-items: center;
    width: 44px;
    padding: 0;
    background: transparent;
    color: var(--muted);
  }

  button:hover, button:focus-visible {
    color: var(--green-dark);
    background: var(--green-soft);
  }

  button:focus-visible { 
    outline-offset: -3px; 
  }
</style>
