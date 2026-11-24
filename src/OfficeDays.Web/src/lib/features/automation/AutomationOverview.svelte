<script lang="ts">
  import AutomationSectionHeader from './AutomationSectionHeader.svelte';
  import { onMount } from 'svelte';
  import { detectPlatform, groups, platforms, type Platform } from './platforms';

  let device = $state<Platform | null>(null);
  onMount((): void => { device = detectPlatform(navigator); });

  let getDescription = (platform : string): string => {
    platform = platform.toLocaleLowerCase();
    switch (platform) {
      case 'ios': 
        return 'Shortcuts';
      case 'macos':
        return 'Shortcuts (coming soon)';
      default:
        return 'Not available yet';
    }
  } 
</script>

<section class="card overview">
  <AutomationSectionHeader heading="h1" title="Automation" icon="calendar"
    description="Automatically mark your days at the office." />
  
  {#if device === 'ios'}
    <a class="shortcut" href="/automation/ios">
      <img src="/automation/shortcuts.webp" alt="" width="24" height="24" /> Automate with Shortcuts
    </a>
  {:else if device}
    <a class="unavailable" href={'/automation/' + device}>{platforms[device]} automation</a>
    <p class="muted">Not yet available. We’re working on it!</p>
  {/if}
  
  <details open={!device}>
    <summary>Other device</summary>
    <div class="groups">
      {#each groups as group}
        <div>
          <h2>{group.name}</h2>
          <ul>
            {#each group.devices as platform}
              <li>
                <a class:unavailable={platform !== 'ios'} href={'/automation/' + platform}>{platforms[platform]}</a>
                <span> — {getDescription(platform)}</span>
              </li>
            {/each}
          </ul>
        </div>
      {/each}
    </div>
  </details>
</section>

<style>
  .overview { 
    margin-bottom: 1rem; 
  }
  
  .overview > p { 
    margin-bottom: .7rem; 
  }
  
  .shortcut { 
    display: inline-flex; 
    align-items: center; 
    gap: .55rem; 
    min-height: 44px; 
    font-weight: 650; 
  }
  
  .shortcut img { 
    border-radius: .35rem; 
  }
  
  details { 
    margin-top: .75rem; 
    border-top: 1px solid var(--line); 
    padding-top: .4rem; 
  }
  
  summary { 
    width: fit-content; 
    cursor: pointer; 
    padding-block: .65rem; 
    font-weight: 650; 
  }
  
  .groups { 
    display: flex; 
    flex-wrap: wrap; 
    gap: .7rem 2rem; 
  }
  
  ul { 
    list-style: none; 
    padding: 0; 
    margin: 0; 
  }
  
  a.unavailable + span { 
    color: var(--muted); 
  }

  li a { 
    display: inline-block; 
    padding-block: .55rem; 
  }
</style>
