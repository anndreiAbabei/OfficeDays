<script lang="ts">
  import type { Status } from '$lib/types';
  
  let { status = null, loading = false, plannedCount = null }: { status?: Status | null; loading?: boolean; plannedCount?: number | null } = $props();
  let upcomingPlanned = $derived(plannedCount ?? status?.upcomingPlannedDays);
  let daysToPlan = $derived(status && upcomingPlanned !== undefined 
          ? Math.max(0, status.remainingOfficeDays - upcomingPlanned) 
          : null);
  let planningMessage = $derived(daysToPlan === null ? ''
    : status?.remainingOfficeDays === 0 ? 'Office requirement met.'
    : daysToPlan === 0 ? 'All remaining days planned.'
    : `${daysToPlan} more ${daysToPlan === 1 ? 'day' : 'days'} to plan.`);
  let progress: number = $derived(Math.max(0, Math.min(100, status?.progressPercentage ?? 0)));
</script>

<section class="status-grid" aria-label="Monthly status" aria-busy={loading}>
  <article class="metric card">
    <span>Required office</span>
    <div class="metric-value">
      <strong>{status?.requiredOfficeDays ?? '—'}</strong>
      <small>days</small>
    </div>
  </article>
  <article class="metric card">
    <span>Office so far</span>
    <div class="metric-value">
      <strong>{status?.officeDays ?? '—'}</strong>
      <small>eligible days</small>
    </div>
  </article>
  <article class="metric card accent">
    <span>Remaining</span>
    <div class="metric-value">
      <strong>{status?.remainingOfficeDays ?? '—'}</strong>
      <small>days</small>
    </div>
  </article>
  <article class="metric card">
    <span>Upcoming planned</span>
    <div class="metric-value">
      <strong class="planning-ratio" aria-label={`${upcomingPlanned ?? 'Unknown'} planned / ${status?.remainingOfficeDays ?? 'unknown'} remaining office days`}>{upcomingPlanned ?? '—'} / {status?.remainingOfficeDays ?? '—'}</strong>
    </div>
    {#if planningMessage}
      <p class="planning-hint" aria-live="polite">{planningMessage}</p>
    {/if}
  </article>
</section>
<section class="card progress-card" aria-busy={loading}>
  <div class="row spread">
    <span>Monthly progress</span>
    <span>{status ? `${status.progressPercentage}%` : '—'}</span>
  </div>
  <div class="progress" role="progressbar" aria-label="Office attendance progress" aria-valuemin="0" aria-valuemax="100" aria-valuenow={status ? progress : undefined}>
    <div style:width={`${progress}%`}></div>
  </div>
  <p class="muted">
    {status ? `${status.eligibleWorkingDays} eligible working days · ${status.requiredOfficeDays} work from office days · ${status.maximumWfhDays} max WFH days` : 'Loading monthly summary…'}
  </p>
</section>

<style>
  .spread { 
    justify-content: space-between; 
  }
  .status-grid { 
    display: grid; 
    grid-template-columns: repeat(4, 1fr);
    gap: 1rem;
  }
  .metric { 
    display: grid; 
    grid-template-columns: 1fr;
    align-items: baseline; 
    column-gap: .5rem; 
    row-gap: .25rem; 
  }
  .metric span { 
    grid-column: 1 / -1; 
    color: var(--muted); 
    font-size: .8rem; 
    font-weight: 650; 
  }
  .metric-value {
    display: flex;
    align-items: baseline;
    gap: .65rem;
  }
  .metric strong { 
    font-size: 2.5rem;
    letter-spacing: -.15em;
    font-variant-numeric: tabular-nums;
  }
  .metric .planning-ratio {
    font-size: clamp(1.7rem, 3vw, 2.5rem);
    letter-spacing: normal;
    white-space: nowrap;
  }
  .planning-hint {
    grid-column: 1 / -1;
    margin: .2rem 0 0;
    color: var(--muted);
    font-size: .8rem;
  }
  .metric small { 
    color: var(--muted); 
  }
  .metric.accent { 
    color: var(--green-dark); 
    background: var(--green-soft); 
    border-color: #c6dfd0; 
  }
  .progress-card { 
    margin: 1rem 0; 
  }
  .progress { 
    height: .65rem; 
    margin: .7rem 0; 
    overflow: hidden; 
    border-radius: 999px;
    background: var(--progress-track); 
  }
  .progress div { 
    width: 0; 
    height: 100%; 
    border-radius: inherit; 
    background: var(--green); 
    transition: width .2s ease; 
  }
  @media (max-width: 650px) {
    .status-grid { grid-template-columns: 1fr; gap: .6rem; }
    .metric { grid-template-columns: minmax(0, 1fr) auto; }
    .metric span { grid-column: auto; }
    .metric-value { justify-self: end; }
    .metric strong, .metric .planning-ratio { font-size: 1.7rem; }
    .metric small { display: none; }
  }
  @media (prefers-reduced-motion: reduce) {
    .progress div { transition: none; }
  }
</style>
