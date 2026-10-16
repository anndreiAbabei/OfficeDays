<script lang="ts">
  import { user } from '$lib/session';
  import Reports from '$lib/features/Reports.svelte';
</script>

<svelte:head>
  <title>Reports · Office Days</title>
</svelte:head>

<h1>Reports</h1>
<Reports timeZone={$user!.timeZoneId} />
