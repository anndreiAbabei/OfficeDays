<script lang="ts">
  import { createSessionUpdater, user } from '$lib/session';
  import AttendanceRequirements from '$lib/features/AttendanceRequirements.svelte';
  import DeleteAccount from '$lib/features/DeleteAccount.svelte';
  import Profile from '$lib/features/Profile.svelte';
  import PersonalDataDownload from '$lib/features/PersonalDataDownload.svelte';
  const updateSession = createSessionUpdater();
</script>

<svelte:head>
    <title>Profile · Office Days</title>
</svelte:head>

<h1>Profile</h1>
<div class="stack">
  <Profile currentUser={$user!} onsave={updateSession} />
  <AttendanceRequirements onsave={updateSession} />
  <PersonalDataDownload />
  <DeleteAccount />
</div>
