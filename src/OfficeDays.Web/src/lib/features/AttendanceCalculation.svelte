<script lang="ts">
  import { displayDate } from '$lib/dates';
  import type { AttendanceStatus } from './attendanceSummary';

  let { status, upcomingPlanned }: { status: AttendanceStatus; upcomingPlanned: number } = $props();
  let summary = $derived(status.summary);
  let percentage = $derived(summary.requirementValue * (summary.requirementMode === 'DaysPerWeek' ? 20 : 1));

  function days(count: number): string {
    return `${count} ${count === 1 ? 'day' : 'days'}`;
  }

  function dateNote(date: string, vacation = false): string {
    if ([0, 6].includes(new Date(`${date}T00:00:00Z`).getUTCDay()))
      return 'Weekend; no deduction';
    if (vacation && summary.bankHolidays.some((holiday): boolean => holiday.date === date))
      return 'Bank holiday; already excluded';
    return '';
  }
</script>

<div class="calculation-grid">
  <section aria-label="Calendar calculation">
    <h3>Calendar days</h3>
    <dl>
      <div><dt>Total days</dt><dd>{summary.totalDays} <span class="muted">including weekends</span></dd></div>
      <div><dt>Working days</dt><dd>{summary.workingDays} <span class="muted">Monday–Friday</span></dd></div>
      <div>
        <dt>Bank holidays</dt>
        <dd>
          {days(summary.bankHolidays.length)} · {summary.bankHolidayWorkingDays} {summary.bankHolidayWorkingDays === 1 ? 'weekday' : 'weekdays'} excluded
          {#if summary.bankHolidays.length}
            <ul aria-label="Bank holiday dates">
              {#each summary.bankHolidays as holiday}
                <li><time datetime={holiday.date}>{displayDate(holiday.date)}</time> — {holiday.name}
                  {#if dateNote(holiday.date)}<small class="muted">{dateNote(holiday.date)}</small>{/if}
                </li>
              {/each}
            </ul>
          {:else}<p class="muted empty">No bank holidays this month.</p>{/if}
        </dd>
      </div>
      <div>
        <dt>Vacation</dt>
        <dd>
          {days(summary.vacationDates.length)} · {summary.vacationWorkingDays} additional {summary.vacationWorkingDays === 1 ? 'weekday' : 'weekdays'} excluded
          {#if summary.vacationDates.length}
            <ul aria-label="Vacation dates">
              {#each summary.vacationDates as date}
                <li><time datetime={date}>{displayDate(date)}</time>
                  {#if dateNote(date, true)}<small class="muted">{dateNote(date, true)}</small>{/if}
                </li>
              {/each}
            </ul>
          {:else}<p class="muted empty">No vacation this month.</p>{/if}
        </dd>
      </div>
    </dl>
    <p class="formula">{summary.workingDays} weekdays − {summary.bankHolidayWorkingDays} bank holiday {summary.bankHolidayWorkingDays === 1 ? 'weekday' : 'weekdays'} − {summary.vacationWorkingDays} vacation {summary.vacationWorkingDays === 1 ? 'weekday' : 'weekdays'} = <strong>{status.eligibleWorkingDays} eligible working days</strong>.</p>
    <p class="muted">Weekends and overlapping absences are never deducted twice. Only dates in this month are listed.</p>
  </section>

  <section aria-label="Office target calculation">
    <h3>Office target</h3>
    <dl>
      <div><dt>Attendance requirement</dt><dd>{summary.requirementMode === 'DaysPerWeek' ? `${summary.requirementValue} days per week (${percentage}%)` : `${percentage}%`}</dd></div>
      <div><dt>Required office days</dt><dd>{days(status.requiredOfficeDays)}</dd></div>
    </dl>
    <p class="formula">{status.eligibleWorkingDays} eligible working days × {percentage}% = <strong>{days(status.requiredOfficeDays)}</strong>, rounded up to a whole day.</p>
    {#if summary.bankHolidayWorkingDays + summary.vacationWorkingDays > 0}
      <p class="comparison">Without holidays or vacation, your target would be {summary.requiredOfficeDaysWithoutAbsences} office {summary.requiredOfficeDaysWithoutAbsences === 1 ? 'day' : 'days'} out of {summary.workingDays} weekdays. With these absences, it is {days(status.requiredOfficeDays)}.</p>
    {/if}
    {#if summary.requirementMode === 'DaysPerWeek'}
      <p class="muted">Each day per week is equivalent to 20% of the month's eligible working days. The target is rounded once for the month.</p>
    {/if}

    <h3>Attendance and plans</h3>
    <dl>
      <div><dt>Planned days this month</dt><dd>{days(summary.plannedDays)}</dd></div>
      <div><dt>Upcoming planned days</dt><dd>{days(upcomingPlanned)}</dd></div>
      <div><dt>Office days recorded</dt><dd>{days(status.officeDays)}</dd></div>
      <div><dt>Office days left to attend</dt><dd>{days(status.remainingOfficeDays)}</dd></div>
      <div><dt>Available days left</dt><dd>{days(summary.remainingEligibleDays)}</dd></div>
      <div class:outstanding={summary.outstandingOfficeDays > 0}><dt>Unavoidable outstanding days</dt><dd>{days(summary.outstandingOfficeDays)}</dd></div>
    </dl>
    <p class="muted">Planned totals include eligible plans for the whole month. Upcoming plans exclude attended and past dates.</p>
    <p class="muted">As of <time datetime={summary.today}>{displayDate(summary.today)}</time> in your timezone. Available days include today if eligible and not already attended. Only eligible attendance counts; plans do not count as attendance.</p>
    {#if summary.outstandingOfficeDays > 0}
      <p class="outstanding-message">{days(status.remainingOfficeDays)} still required − {days(summary.remainingEligibleDays)} available = <strong>{days(summary.outstandingOfficeDays)} that cannot be covered this month</strong>.</p>
    {:else}
      <p class="muted">{status.remainingOfficeDays === 0 ? 'Your office requirement is met.' : 'There are enough eligible days left to meet your target.'}</p>
    {/if}
  </section>
</div>

<style>
  .calculation-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 2rem; border-top: 1px solid var(--line); margin-top: 1rem; padding-top: .5rem; }
  section { min-width: 0; overflow-wrap: anywhere; }
  h3 { font-size: 1rem; margin: 1rem 0 .6rem; }
  dl { margin: 0; font-size: .9rem; }
  dl > div { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); gap: .75rem; padding: .55rem 0; border-bottom: 1px solid var(--line); }
  dt { font-weight: 650; }
  dd { margin: 0; font-variant-numeric: tabular-nums; }
  ul { padding-left: 1.2rem; margin: .5rem 0 0; }
  li { margin: .4rem 0; }
  small { display: block; }
  p { font-size: .85rem; line-height: 1.6; }
  .empty { margin: .3rem 0 0; }
  .formula, .comparison, .outstanding-message { padding: .75rem; background: var(--progress-track); border-radius: .5rem; }
  .outstanding { font-weight: 700; }
  @media (max-width: 800px) {
    .calculation-grid { grid-template-columns: 1fr; gap: 0; }
  }
  @media (max-width: 400px) {
    dl > div { grid-template-columns: 1fr; gap: .25rem; }
  }
</style>
