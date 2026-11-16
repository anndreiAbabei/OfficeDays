<script lang="ts">
  import type { Snippet } from 'svelte';
  import type { Appearance, TeamDay } from '$lib/types';
  import AppearanceSelect from './AppearanceSelect.svelte';

  let { teamDay = $bindable(), appearance = $bindable(), saveLabel, busy = false, onsave, children }: {
    teamDay: TeamDay | null;
    appearance: Appearance;
    saveLabel: string;
    busy?: boolean;
    onsave: () => void;
    children?: Snippet;
  } = $props();

  const weekdays: TeamDay[] = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday'];
  const helpId = $props.id();
</script>

<form class="stack" onsubmit={(event): void => { event.preventDefault(); onsave(); }}>
  <div class="fields">
    {@render children?.()}
    <label>
      Team day
      <select bind:value={teamDay} aria-describedby={helpId}>
        <option value={null}>None</option>
        {#each weekdays as day}
          <option value={day}>{day[0].toUpperCase() + day.slice(1)}</option>
        {/each}
      </select>
    </label>
    <AppearanceSelect bind:value={appearance} />
  </div>
  <div class="actions">
    <small id={helpId}>Your team day is highlighted in your calendar.</small>
    <button type="submit" disabled={busy}>{busy ? 'Saving…' : saveLabel}</button>
  </div>
</form>

<style>
  .fields { 
    display: grid; 
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 11rem), 1fr)); 
    gap: .9rem; 
    max-width: 48rem; 
  }
  
  .actions { 
    display: flex; 
    align-items: center; 
    justify-content: space-between; 
    gap: 1rem; 
  }
  
  small {
    color: var(--muted); 
  }
  
  button { 
    flex: none; 
  }
</style>
