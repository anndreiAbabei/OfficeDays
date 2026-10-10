<script lang="ts">
  import { months, shiftPeriod } from '$lib/dates';
  import type { Period } from '$lib/types';
  
  let { period, onchange }: { period: Period; onchange: (period: Period) => void } = $props();
  
  function yearChanged(event: Event): void {
    const input = event.currentTarget as HTMLInputElement;
    if (input.checkValidity() && input.value) 
      onchange({ ...period, year: Number(input.value) });
  }
</script>

<section class="period-picker card compact" aria-label="Selected month">
  <button class="icon secondary" aria-label="Previous month" disabled={period.year === 1 && period.month === 1} onclick={() => onchange(shiftPeriod(period, -1))}>
    ←
  </button>
  
  <label>
    Month
    <select value={period.month} onchange={event => onchange({ ...period, month: Number(event.currentTarget.value) })}>
      {#each months as name, i}<option value={i + 1}>{name}</option>{/each}
    </select>
  </label>
  
  <label>
    Year
    <input type="number" min="1" max="9999" required value={period.year} onchange={yearChanged} />
  </label>
  
  <button class="icon secondary" aria-label="Next month" disabled={period.year === 9999 && period.month === 12} onclick={() => onchange(shiftPeriod(period, 1))}>
    →
  </button>
</section>

<style>
  .period-picker { 
    display: grid; 
    grid-template-columns: auto 1fr 7rem auto; 
    gap: .7rem; 
    align-items: end; 
    margin-bottom: 1rem; 
  }
  
  .period-picker .icon { 
    min-height: 2.55rem; 
  }
  
  @media (max-width: 650px) {
    .period-picker {
      grid-template-columns: auto 1fr auto; 
    }
    .period-picker label:last-of-type { 
      grid-column: 2; 
      grid-row: 2; 
    }
  }
</style>
