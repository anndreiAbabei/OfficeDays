<script lang="ts">
  import Feedback from '$lib/components/Feedback.svelte';

  let { label, value, action }: { label: string; value: string; action: string } = $props();
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
  <label>{label}<input readonly value={value} bind:this={input} spellcheck="false" /></label>
  <button type="button" class="secondary" onclick={copy}>{action}</button>
</div>
<Feedback {message} {error} />

<style>
  .field { 
    display: grid; 
    grid-template-columns: minmax(0, 1fr) auto; 
    align-items: end; 
    gap: .4rem;
    margin-top: .9rem; 
  }
</style>
