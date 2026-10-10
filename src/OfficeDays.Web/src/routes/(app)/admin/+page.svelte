<script lang="ts">
  import { user } from '$lib/session';
  import Administration from '$lib/features/Administration.svelte';
</script>

<svelte:head>
    <title>Administration · Office Days</title>
</svelte:head>

<h1>Administration</h1>
{#if $user?.isAdmin}
    <Administration countryCode={$user.countryCode} timeZone={$user.timeZoneId} />
{:else}
    <section class="card">
        <p role="alert">Administrator access is required.</p>
        <a href="/">Return to dashboard</a>
    </section>
{/if}
