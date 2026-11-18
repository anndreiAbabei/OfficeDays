<script lang="ts">
  import { onMount } from 'svelte';
  import { request } from '$lib/api';

  type AppVersion = {
    version: string;
    latestVersion: string;
    newVersionAvailable: boolean;
    author: string;
    gitHubRepo: string;
  };

  let release = $state<AppVersion>();
  let status = $state('Loading version…');

  async function loadVersion(): Promise<void> {
    try {
      release = await request<AppVersion>('/api/version');
    } catch {
      status = 'Version unavailable';
    }
  }

  onMount(() => { void loadVersion(); });
</script>

<div class="base">
  <span>Created by <a href={release?.gitHubRepo ?? '#'}>{release?.author ?? 'Andrei'}</a> · </span>
  <span>
    Office Days ·
    {#if release?.newVersionAvailable}
      <strong class="new-version" title={`${release.latestVersion} version is available`}>v{release.version}</strong>
    {:else}
      {release ? `v${release.version}` : status}
    {/if}
  </span>
</div>

<style>
  strong.new-version {
    color: var(--orange-warn); 
  }
  
  div.base {
    opacity: .5;
  }

  div.base {
    opacity: initial;
  }
</style>
