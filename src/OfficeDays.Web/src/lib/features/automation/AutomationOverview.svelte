<script lang="ts">
  import { onMount } from 'svelte';
  import { detectPlatform, groups, platforms, type Platform } from './platforms';

  let device = $state<Platform | null>(null);
  onMount((): void => { device = detectPlatform(navigator); });
</script>

<section class="card overview">
  <header>
    <span class="calendar" aria-hidden="true"><span>✓</span></span>
    <div>
      <h1>Automation</h1>
      <p>Automatically mark your days at the office.</p>
    </div>
  </header>
  
  {#if device === 'ios'}
    <a class="shortcut" href="/automation/ios">
      <img src="/automation/shortcuts.webp" alt="" width="24" height="24" /> Automate with Shortcuts
    </a>
  {:else if device}
    <a href={'/automation/' + device}>{platforms[device]} automation</a>
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
                <a href={'/automation/' + platform}>
                  {platforms[platform]} — {platform === 'ios' ? 'Shortcuts' : platform === 'macos' ? 'Shortcuts (coming soon)' : 'Not available yet'}
                </a>
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
  
  header { 
    display: flex; 
    align-items: center; 
    gap: .85rem; 
    margin-bottom: .75rem; 
  }
  
  header p { 
    margin-bottom: 0; 
  }
  
  .calendar {
    flex: none; 
    width: 3rem; 
    height: 3rem; 
    border: 2px solid var(--green); 
    border-top-width: .6rem;
    border-radius: .55rem; 
    background: var(--green-soft); 
    color: var(--green-dark); 
    display: grid; 
    place-items: center; 
  }
  
  .calendar span { 
    font-size: 1.6rem; 
    font-weight: 750; 
    line-height: 1; 
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
  
  li a { 
    display: inline-block; 
    padding-block: .55rem; 
  }
</style>
