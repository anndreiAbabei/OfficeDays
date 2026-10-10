<script lang="ts">
  import { user } from '$lib/session';
  import { localDate, months } from '$lib/dates';
  import PeriodPicker from '$lib/components/PeriodPicker.svelte';
  import Dashboard from '$lib/features/Dashboard.svelte';
  import type { Period } from '$lib/types';
  let period = $state<Period>(localDate($user!.timeZoneId));
</script>

<svelte:head>
  <title>Dashboard · Office Days</title>
</svelte:head>

<h1>{months[period.month - 1]} {period.year}</h1>

<PeriodPicker {period} onchange={value => period = value} />

{#key `${period.year}-${period.month}`}
  <Dashboard {period} timeZone={$user!.timeZoneId} />
{/key}
