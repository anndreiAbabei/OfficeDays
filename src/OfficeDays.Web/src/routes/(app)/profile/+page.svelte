<script lang="ts">
  import { user } from '$lib/session';
  import Profile from '$lib/features/Profile.svelte';
</script>
<svelte:head><title>Profile · Office Days</title></svelte:head>
<h1>Profile</h1>
<Profile currentUser={$user!} onsave={value => user.set(value)} />
