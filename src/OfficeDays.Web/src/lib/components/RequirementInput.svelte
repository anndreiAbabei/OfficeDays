<script lang="ts">
  import type { AttendanceRequirementMode } from '$lib/attendanceRequirementMode';
  let { requirementMode = $bindable<AttendanceRequirementMode>('Percentage'), requirementValue = $bindable<number | undefined>(50), disabled = false }: {
    requirementMode: AttendanceRequirementMode; requirementValue: number | undefined; disabled?: boolean;
  } = $props();

  function changeMode(): void {
    requirementValue = requirementMode === 'DaysPerWeek' ? 3 : 50;
  }
</script>

<label>
  Requirement type
  <select bind:value={requirementMode} onchange={changeMode} {disabled}>
    <option value="Percentage">Percentage</option>
    <option value="DaysPerWeek">Days per week</option>
  </select>
</label>
<label>
  {requirementMode === 'DaysPerWeek' ? 'Office days per week' : 'Required office days (%)'}
  <input type="number" min="0" max={requirementMode === 'DaysPerWeek' ? 5 : 100} step="1" required bind:value={requirementValue} {disabled} />
</label>
{#if requirementMode === 'DaysPerWeek'}
  <p class="muted">
    Averaged over the month, adjusted for holidays and vacations.
    Choose any eligible days to meet your monthly quota. The total is rounded up once per month.
  </p>
{/if}
