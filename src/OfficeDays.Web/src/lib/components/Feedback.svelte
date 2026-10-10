<script lang="ts">
  let { error = '', message = '' }: { error?: string; message?: string } = $props();
</script>
{#if error}<p class="error" role="alert">{error}</p>{/if}
{#if message}<p class="message" role="status">{message}</p>{/if}

<style>
  .message { 
      min-height: 1.5rem; 
      margin: 0 0 .5rem; 
      color: var(--green-dark); 
      font-size: .9rem;
  }
  
  .message { 
      margin-top: .7rem; 
      overflow-wrap: anywhere; 
  }
</style>
