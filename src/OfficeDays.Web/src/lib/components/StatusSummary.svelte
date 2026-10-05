<script lang="ts">
  import type { Status } from '$lib/types';
  let { status = null, loading = false }: { status?: Status | null; loading?: boolean } = $props();
  let progress = $derived(Math.max(0, Math.min(100, status?.progressPercentage ?? 0)));
</script>
<section class="status-grid" aria-label="Monthly status" aria-busy={loading}>
  <article class="metric card"><span>Required office</span><strong>{status?.requiredOfficeDays ?? '—'}</strong><small>days</small></article>
  <article class="metric card"><span>Office so far</span><strong>{status?.officeDays ?? '—'}</strong><small>eligible days</small></article>
  <article class="metric card accent"><span>Remaining</span><strong>{status?.remainingOfficeDays ?? '—'}</strong><small>days</small></article>
</section>
<section class="card progress-card" aria-busy={loading}>
  <div class="row spread"><span>Monthly progress</span><span>{status ? `${status.progressPercentage}%` : '—'}</span></div>
  <div class="progress" role="progressbar" aria-label="Office attendance progress" aria-valuemin="0" aria-valuemax="100" aria-valuenow={status ? progress : undefined}>
    <div style:width={`${progress}%`}></div>
  </div>
  <p class="muted">{status ? `${status.eligibleWorkingDays} eligible working days · ${status.requiredOfficeDays} work from office days · ${status.maximumWfhDays} max WFH days` : 'Loading monthly summary…'}</p>
</section>
