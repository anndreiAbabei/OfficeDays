<script lang="ts">
  let { mode = $bindable('Percentage'), value = $bindable<number | undefined>(50), disabled = false }: {
    mode: string; value: number | undefined; disabled?: boolean;
  } = $props();

  function changeMode(): void {
    value = mode === 'DaysPerWeek' ? 3 : 50;
  }
</script>

<label>
  Requirement type
  <select bind:value={mode} onchange={changeMode} {disabled}>
    <option value="Percentage">Percentage</option>
    <option value="DaysPerWeek">Days per week</option>
  </select>
</label>
<label>
  {mode === 'DaysPerWeek' ? 'Office days per week' : 'Required office days (%)'}
  <input type="number" min="0" max={mode === 'DaysPerWeek' ? 5 : 100} step="1" required bind:value {disabled} />
</label>
{#if mode === 'DaysPerWeek'}
  <p class="muted">
    Averaged over the month, adjusted for holidays and vacations.
    Choose any eligible days to meet your monthly quota. The total is rounded up once per month.
  </p>
{/if}
