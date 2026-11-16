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
    grid-template-columns: 44px minmax(0, 1fr) 5rem 44px;
    align-items: end;
    gap: .5rem;
    margin-bottom: 1rem;
    width: 100%;
  }
  
  label { 
    min-width: 0; 
  }
  
  @media (max-width: 650px) {
    input { 
      appearance: textfield; 
    }
    
    input::-webkit-inner-spin-button, input::-webkit-outer-spin-button { 
      appearance: none; 
      margin: 0; 
    }
  }

  @media (max-width: 400px) {
    .period-picker {
      gap: .25rem; 
      padding: .5rem; 
      grid-template-columns: 44px minmax(0, 1fr) 4.25rem 44px;
    }
    
    input { 
      padding-inline: .25rem; 
    }
    
    select { 
      padding-left: .5rem; 
    }
  }
</style>
