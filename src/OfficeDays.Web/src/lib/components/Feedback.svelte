<script lang="ts">
  let { error = '', message = '' }: { error?: string; message?: string } = $props();
</script>
{#if error}<p class="error" role="alert">{error}</p>{/if}
{#if message}<p class="message" role="status">{message}</p>{/if}
