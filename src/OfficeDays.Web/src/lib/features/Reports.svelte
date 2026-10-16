<script lang="ts">
  import { onMount, untrack } from 'svelte';
  import { request, errorMessage } from '$lib/api';
  import { localDate, months } from '$lib/dates';
  import Feedback from '$lib/components/Feedback.svelte';
  import { downloadReport, resultLabels, type AttendanceReport } from './reports';

  let { timeZone }: { timeZone: string } = $props();
  let year = $state<number | undefined>(untrack(() => localDate(timeZone).year));
  let startMonth = $state(1); 
  let endMonth = $state(12);
  let report = $state<AttendanceReport | null>(null);
  let loading = $state(false); 
  let error = $state('');
  let filtersChanged = $derived(!!report && (report.year !== year || report.startMonth !== startMonth || report.endMonth !== endMonth));
  let scale = $derived(Math.max(1, ...(report?.months.flatMap(month => [month.officeDays ?? 0, month.requiredOfficeDays]) ?? [])));
  let exportEnabled = $derived(!!report?.months.length && !loading && !filtersChanged);

  function monthName(date: string): string {
    return `${months[Number(date.slice(5, 7)) - 1]} ${Number(date.slice(0, 4))}`;
  }
  
  function generatedTime(value: string, zone: string): string {
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'medium', timeZone: zone }).format(new Date(value));
  }
  
  async function load(event?: SubmitEvent): Promise<void> {
    event?.preventDefault();
    
    if (loading) 
      return;
    
    error = '';
    
    if (!year || !Number.isInteger(year) || year < 1 || year > 9998 || startMonth > endMonth) {
      error = 'Choose a valid year and an ending month on or after the starting month.';
      return;
    }
    
    loading = true;
    report = null;
    try {
      report = await request<AttendanceReport>(`/api/reports?year=${year}&startMonth=${startMonth}&endMonth=${endMonth}`);
    } catch (failure) { 
      error = errorMessage(failure); 
    }
    finally { 
      loading = false; 
    }
  }
  
  function exportCsv(): void {
    if (!exportEnabled || !report) 
      return;
    
    try { 
      downloadReport(report); 
    }
    catch (failure) { 
      error = errorMessage(failure); 
    }
  }
  onMount(() => { void load(); });
</script>

<div class="stack reports">
  <section class="card">
    <h2>Your attendance report</h2>
    <p class="muted">Review your recorded office attendance and monthly requirements.</p>
    
    <form class="filters" onsubmit={load}>
      <label>
        Year 
        <input type="number" min="1" max="9998" step="1" required bind:value={year} disabled={loading} />
      </label>
      <label>
        From month
        <select bind:value={startMonth} disabled={loading}>
          {#each months as month, index}<option value={index + 1}>{month}</option>{/each}
        </select>
      </label>
      <label>
        To month
        <select bind:value={endMonth} disabled={loading}>
          {#each months as month, index}<option value={index + 1}>{month}</option>{/each}
        </select>
      </label>
      <button type="submit" disabled={loading}>
        {loading ? 'Generating…' : filtersChanged || !report ? 'Generate report' : 'Refresh report'}
      </button>
      <button class="secondary" type="button" disabled={!exportEnabled} onclick={exportCsv}>
        Export CSV
      </button>
    </form>
    
    {#if filtersChanged}
      <p class="muted" role="status">Filters changed. Generate the report to apply them.</p>
    {/if}
    
    <Feedback {error} />
  </section>

  {#if loading}
    <section class="card loading" role="status" aria-live="polite">
      <span class="spinner" aria-hidden="true"></span> 
      Generating your report…
    </section>
  {:else if report}
    <div class="report-context">
      <h2>
        {months[report.startMonth - 1]}–{months[report.endMonth - 1]} {report.year}
      </h2>
      <p class="muted">
        {report.username} · Generated {generatedTime(report.generatedAt, report.timeZoneId)} ({report.timeZoneId})
      </p>
      <p class="muted">
        Reports begin in {monthName(report.registrationMonth)}, your registration month. 
        Export uses the data shown here; refresh to include later changes.
      </p>
    </div>
    
    {#if !report.months.length}
      <section class="card">
        <h2>No months in this range</h2>
        <p class="muted">
          Choose a range on or after {monthName(report.registrationMonth)}.
        </p>
      </section>
    {:else}
      <section class="summary" aria-label="Report summary">
        <article class="card">
          <h3>Recorded office days</h3>
          <strong>{report.summary.officeDays}</strong>
          <p class="muted">Across started months</p>
        </article>
        <article class="card">
          <h3>Office attendance</h3>
          <strong>{report.summary.attendancePercentage === null ? '—' : `${report.summary.attendancePercentage}%`}</strong>
          <p class="muted">Share of full-month eligible days in started months</p>
        </article>
        <article class="card">
          <h3>Months meeting target</h3>
          <strong>{report.summary.monthsMeetingTarget} <small>of {report.summary.completedMonths}</small></strong>
          <p class="muted">Completed months only</p>
        </article>
      </section>
 
      <section class="card" aria-labelledby="comparison-heading">
        <h2 id="comparison-heading">
          Office days by month
        </h2>
        <p class="legend">
          <span><i class="actual-key"></i> Recorded</span>
          <span><i class="target-key"></i> Required</span>
        </p>
       
        <div class="chart" aria-label="Recorded versus required office days; exact values in monthly breakdown">
          {#each report.months as month}
            <div class="chart-row">
              <span class="month-label">
                {months[month.month - 1].slice(0, 3)}
              </span>
              <div class="bars" aria-hidden="true">
                <div class="bar-track">
                  <div class="actual-bar" style:width={`${(month.officeDays ?? 0) / scale * 100}%`}></div>
                </div>
                <div class="bar-track">
                  <div class="target-bar" style:width={`${month.requiredOfficeDays / scale * 100}%`}></div>
                </div>
              </div>
              <span class="chart-value">
                {month.officeDays ?? '—'} / {month.requiredOfficeDays}
                <small>
                  {month.result === 'notStarted' ? 'Not started' : month.result === 'inProgress' ? 'In progress' : ''}
                </small>
              </span>
            </div>
          {/each}
        </div>
      </section>
      <section class="card">
        <h2 id="breakdown-heading">Monthly breakdown</h2>
        <p class="muted">
          The current month uses its full monthly target and is marked “In progress”. 
          Future months show scheduled requirements and are excluded from summary totals.
        </p>
        <!-- svelte-ignore a11y_no_noninteractive_tabindex (Keyboard users need to focus and scroll the wide table.) -->
        <div class="table-scroll" tabindex="0" role="region" aria-labelledby="breakdown-heading">
          <table>
            <thead>
              <tr>
                <th scope="col">Month</th>
                <th scope="col">Eligible days</th>
                <th scope="col">Required %</th>
                <th scope="col">Required days</th>
                <th scope="col">Recorded days</th>
                <th scope="col">Attendance %</th>
                <th scope="col">Result</th>
              </tr>
            </thead>
            <tbody>
              {#each report.months as month}
                <tr>
                  <th scope="row">{months[month.month - 1]}</th>
                  <td>{month.eligibleWorkingDays}</td>
                  <td>{month.requiredOfficePercentage}%</td>
                  <td>{month.requiredOfficeDays}</td>
                  <td>{month.officeDays ?? '—'}</td>
                  <td>{month.attendancePercentage === null ? '—' : `${month.attendancePercentage}%`}</td>
                  <td><span class="result" class:met={month.result === 'targetMet'} class:below={month.result === 'belowTarget'}>{resultLabels[month.result]}</span></td>
                </tr>
              {/each}
            </tbody>
          </table>
        </div>
        <p class="muted footnote">
          Eligible days exclude weekends, holidays, and vacation. Recorded days count eligible attendance only. 
          “—” means not started or no eligible days for a percentage.
        </p>
      </section>
    {/if}
  {/if}
</div>

<style>
  .reports { 
    grid-template-columns: minmax(0, 1fr); 
    min-width: 0; 
  }
  
  .reports > * { 
    min-width: 0; 
  }
  
  .report-context { 
    overflow-wrap: anywhere; 
  }
  
  .filters { 
    display: grid; 
    grid-template-columns: minmax(6rem, .7fr) repeat(2, minmax(8rem, 1fr)) auto auto; 
    gap: .75rem; 
    align-items: end; 
    margin-top: 1rem; 
  }
  
  .report-context h2 { 
    margin-bottom: .4rem; 
  }
  .report-context p { 
    margin: .3rem 0; 
  }
  
  .summary { 
    display: grid; 
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 1rem; 
  }
  
  .summary h3 { 
    font-size: .85rem; 
    color: var(--muted); 
    margin: 0 0 .6rem; 
  }
  
  .summary strong { 
    font-size: 2rem; 
    font-variant-numeric: tabular-nums; 
  }
  
  .summary small { 
    font-size: 1rem; 
    font-weight: 500; 
    color: var(--muted); 
  }
  
  .summary p { 
    font-size: .8rem; 
    margin: .5rem 0 0; 
  }
  
  .legend { 
    display: flex; 
    gap: 1.2rem; 
    color: var(--muted); 
    font-size: .85rem; 
  }
  
  .legend span { 
    display: flex; 
    align-items: center; 
    gap: .4rem; 
  }
  
  .legend i { 
    width: .8rem; 
    height: .8rem; 
    border-radius: .2rem; 
  }
  
  .actual-key, .actual-bar { 
    background: var(--green); 
  }
  
  .target-key, .target-bar { 
    background: #a6b3c1; 
  }
  
  .chart { 
    display: grid;
    gap: .8rem; 
    margin-top: 1rem; 
  }
  
  .chart-row { 
    display: grid; 
    grid-template-columns: 3rem minmax(0, 1fr) 6rem; 
    gap: .8rem; 
    align-items: center; 
  }

  .month-label { 
    font-size: .85rem; 
    font-weight: 600; 
  }
  
  .bars { 
    display: grid; 
    gap: .2rem; 
  }

  .bar-track { 
    height: .5rem; 
    background: var(--progress-track); 
    border-radius: 3px; 
  }

  .actual-bar, .target-bar { 
    height: 100%; 
    border-radius: 3px; 
  }
  
  .chart-value { 
    font-size: .85rem; 
    font-variant-numeric: tabular-nums; 
    text-align: right; 
  }
    
  .chart-value small { 
    display: block; 
    color: var(--muted); 
    font-size: .7rem; 
    min-height: .8rem; 
  }
    
  .table-scroll { 
    overflow-x: auto; 
    max-width: 100%; 
  }
    
  .table-scroll:focus-visible { 
    outline: 3px solid var(--green); 
    outline-offset: 2px; 
  }
    
  table { 
    width: 100%; 
    border-collapse: collapse; 
    font-size: .85rem; 
  }
    
  th, td { 
    padding: .8rem .65rem; 
    text-align: right; 
    border-bottom: 1px solid var(--line); 
  }
    
  th:first-child { 
    text-align: left; 
  }
    
  thead th { 
    color: var(--muted); 
    font-weight: 600; 
  }
    
  th:last-child, td:last-child { 
    text-align: left; 
  }
    
  .result { 
    display: inline-block; 
    padding: .25rem .5rem; 
    border-radius: .4rem; 
    background: var(--secondary); 
    white-space: nowrap; 
  }
    
  .result.met { 
    color: var(--green-dark); 
    background: var(--green-soft); 
  }
    
  .result.below { 
    color: var(--calendar-missed-text); 
    background: var(--calendar-missed-bg); 
  }
    
  .footnote { 
    font-size: .8rem; 
    margin-bottom: 0; 
    margin-top: 1.5rem;
  }
    
  .loading { 
    display: flex; 
    align-items: center; 
    gap: .75rem; 
  }
    
  .spinner { 
    width: 1rem; 
    height: 1rem; 
    border: 2px solid var(--line); 
    border-top-color: var(--green); 
    border-radius: 50%; 
    animation: spin 1s linear infinite; 
  }
    
  @keyframes spin { 
    to { transform: rotate(360deg); } 
  }
  
  @media (max-width: 900px) { 
    .filters { 
      grid-template-columns: repeat(3, minmax(0, 1fr)); 
    } 
  }
  
  @media (max-width: 650px) {
    .filters { 
      grid-template-columns: repeat(2, minmax(0, 1fr)); 
    }
    
    .filters label:first-child { 
      grid-column: 1 / -1; 
    }
    
    .summary { 
      grid-template-columns: 1fr; 
      gap: .6rem; 
    }
    
    .chart-row { 
      grid-template-columns: 2rem minmax(0, 1fr) 5rem; 
      gap: .5rem; 
    }
    
    .table-scroll:focus-visible { 
      outline: 3px solid var(--green); 
      outline-offset: 2px; 
    }
    
    table { 
      min-width: 650px; 
    }
  }
  
  @media (prefers-reduced-motion: reduce) { .spinner { animation: none; } }
</style>
