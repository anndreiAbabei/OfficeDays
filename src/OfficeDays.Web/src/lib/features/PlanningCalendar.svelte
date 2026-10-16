<script lang="ts">
  import { onMount, untrack } from 'svelte';
  import { request, mutate, errorMessage } from '$lib/api';
  import { displayDate, isoDate, localDate, periodQuery } from '$lib/dates';
  import type { Period, PlanningDay, PlanningMonth, TeamDay } from '$lib/types';
  import { describeDay, togglePlan } from './planning';
  import Feedback from '$lib/components/Feedback.svelte';

  const weekdays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  let { period, timeZone, teamDay = null, revision = 0, oncountchange, onchange }: {
    period: Period; timeZone: string; teamDay?: TeamDay | null; revision?: number;
    oncountchange: (count: number) => void; onchange: () => void;
  } = $props();
  let data = $state<PlanningMonth | null>(null);
  let loading = $state(true);
  let busy = $state(false);
  let error = $state('');
  let selectedDate = $state('');
  let generation = 0;
  let offset = $derived((new Date(`${isoDate(period.year, period.month)}T00:00:00Z`).getUTCDay() + 6) % 7);
  const workdays: TeamDay[] = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday'];
  let teamColumn = $derived(teamDay ? workdays.indexOf(teamDay) + 1 : 0);
  let weekRows = $derived(Math.ceil((offset + (data?.days.length ?? 0)) / 7));
  let selected = $derived(data?.days.find(day => day.date === selectedDate));
  
  function dayDescription(day: PlanningDay): string {
    const weekday = new Date(day.date + 'T00:00:00Z').getUTCDay();
    return describeDay(day) + (teamColumn && weekday === teamColumn ? ' · Team day' : '');
  }

  async function load(): Promise<void> {
    const current = ++generation;
    loading = true;
    error = '';
    try {
      const result = await request<PlanningMonth>(`/api/planning?${periodQuery(period)}`);
      if (current === generation) {
        data = result;
        oncountchange(result.upcomingPlannedDays);
      }
    } catch (failure) {
      if (current === generation) 
        error = errorMessage(failure);
    } finally {
      if (current === generation) 
        loading = false;
    }
  }

  $effect(() => {
    revision;
    untrack(() => { void load(); });
  });

  onMount(() => {
    const timer = setInterval(() => {
      const now = localDate(timeZone);
      if (data && data.today !== isoDate(now.year, now.month, now.day) && !busy && !loading) {
        void load();
        onchange();
      }
    }, 30_000);
    
    return () => { clearInterval(timer); generation++; };
  });

  async function select(day: PlanningDay): Promise<void> {
    selectedDate = day.date;
    if (!day.canToggle || busy || loading || !data) return;
    const previous = data;
    const next = togglePlan(day);
    const delta = day.isAttended ? 0 : next.isPlanned ? 1 : -1;
    
    data = { 
      ...data, 
      days: data.days.map(item => item.date === day.date ? next : item), 
      upcomingPlannedDays: data.upcomingPlannedDays + delta 
    };
    
    oncountchange(data.upcomingPlannedDays);
    busy = true;
    error = '';
    
    try {
      await mutate(`/api/planning/${day.date}`, next.isPlanned ? 'PUT' : 'DELETE');
    } catch (failure) {
      data = previous;
      oncountchange(previous.upcomingPlannedDays);
      error = errorMessage(failure);
      busy = false;
      
      return;
    }
    
    busy = false;
    onchange();
    await load();
  }
</script>

<section class="card planning" aria-labelledby="planning-heading" aria-busy={loading || busy}>
  <div class="section-heading">
    <div>
      <h2 id="planning-heading">Plan office days</h2>
      <p class="muted">Tap an eligible day to add or remove a plan. Changes save automatically.</p>
    </div>
  </div>
  
  <Feedback {error} />
  {#if error}
    <button class="secondary small" onclick={load} disabled={busy || loading}>
      Retry calendar
    </button>
  {/if}
  
  {#if !data}
    <p class="muted">{loading ? 'Loading calendar…' : 'Calendar unavailable.'}</p>
  {:else}
    <div class="calendar" aria-label="Office planning calendar">
      {#if teamColumn}
        <div class="team-column" aria-hidden="true" 
             style:grid-column={teamColumn} 
             style:grid-row={`1 / ${weekRows + 2}`}></div>
      {/if}
      {#each weekdays as weekday, column}
        <div class="weekday" style:grid-column={column + 1} style:grid-row={1}>{weekday}</div>
      {/each}
      
      {#each Array(offset) as _, column}
        <div aria-hidden="true" style:grid-column={column + 1} style:grid-row={2}></div>
      {/each}
      
      {#each data.days as day, index (day.date)}
        <button type="button" 
                class="day {day.status}"
                style:grid-column={(offset + index) % 7 + 1}
                style:grid-row={Math.floor((offset + index) / 7) + 2}
                class:today={day.isToday}
                class:ineligible={!day.isEligible} 
                class:readonly={!day.canToggle}
                aria-label={`${displayDate(day.date)} · ${dayDescription(day)}${day.canToggle ? ' · Toggle plan' : ' · Planning unavailable'}`}
                aria-pressed={day.isPlanned} aria-current={day.isToday ? 'date' : undefined}
                title={dayDescription(day)} disabled={busy || loading}
                onfocus={() => selectedDate = day.date}
                onclick={() => select(day)}>
          <span class="number">
            {Number(day.date.slice(-2))}
          </span>
          <span class="symbol" aria-hidden="true">
            {day.status === 'attended' ? '✓' : day.status === 'missed' ? '!' : day.status === 'holiday' ? '−' : day.isPlanned ? '•' : ''}
            {day.isPlanned && (day.status === 'attended' || day.status === 'holiday') ? ' •' : ''}
          </span>
        </button>
      {/each}
    </div>
    <p class="day-details" aria-live="polite">{selected ? `${displayDate(selected.date)} · ${dayDescription(selected)}` : 'Select any day to read its details.'}</p>
  {/if}
  
  <ul class="legend" aria-label="Calendar legend">
    <li><span class="swatch attended">✓</span> Attended</li>
    <li><span class="swatch planned">•</span> Planned</li>
    {#if teamColumn}
      <li><span class="swatch team-day"></span> Team day</li>
    {/if}
    <li><span class="swatch today"></span> Today</li>
    <li><span class="swatch missed">!</span> Planned · day not recorded</li>
    <li><span class="swatch holiday">−</span> Vacation / Holiday</li>
  </ul>
</section>

<style>
  .planning { 
    margin-bottom: 1rem; 
  }
  
  .calendar {
    display: grid; 
    grid-template-columns: repeat(7, minmax(0, 1fr)); 
    gap: .45rem; 
  }
  
  .team-column {
    border: 2px solid alpha(from var(--calendar-team-outline) / 20%);
    border-radius: .7rem;
    margin: -.18rem;
    pointer-events: none;
    z-index: 1;
  }

  .swatch.team-day {
    border: 2px solid alpha(from var(--calendar-team-outline) / 20%);
    background: transparent;
  }

  .weekday { 
    text-align: center; 
    font-size: .8rem; 
    color: var(--muted); 
    padding: .4rem 0; 
  }
  
  .day { 
    position: relative; 
    min-height: 4.5rem; 
    padding: .55rem; 
    display: flex; 
    align-items: flex-start; 
    justify-content: space-between; 
    border: 1px solid var(--line); 
    border-radius: .6rem; 
    background: var(--paper); 
    color: var(--ink); 
    box-shadow: none; 
  }
  
  .day.readonly { 
    cursor: default; 
  }
  
  .day.ineligible.none {
    background: var(--progress-track); 
    color: var(--muted); 
  }
  
  .attended { 
    background: var(--calendar-attended-bg);
    color: var(--calendar-attended-text);
  }
  
  .planned { 
    background: var(--calendar-planned-bg);
    color: var(--calendar-planned-text);
  }
  
  .missed { 
    background: var(--calendar-missed-bg);
    color: var(--calendar-missed-text);
  }
  .holiday { 
    background: var(--calendar-holiday-bg);
    color: var(--calendar-holiday-text);
  }
  
  .day.today, .swatch.today { 
    outline: 2px solid var(--calendar-today-outline);
    outline-offset: 1px; 
  }
  
  .day:focus-visible { 
    outline: 3px solid var(--ink); 
    outline-offset: 3px; 
  }
  
  .today-label { 
    position: absolute; 
    bottom: .25rem; 
    left: .5rem; 
    font-size: .65rem; 
    font-weight: 700; 
  }
  
  .day-details { 
    min-height: 1.5em; 
    font-size: .85rem; 
    margin: .85rem 0; 
  }
  
  .legend { 
    display: flex; 
    flex-wrap: wrap; 
    gap: .65rem 1.2rem; 
    list-style: none; 
    padding: 0; 
    margin: .8rem 0 0; 
    font-size: .8rem; 
  }
  
  .legend li { 
    display: flex; 
    align-items: center; 
    gap: .4rem; 
  }
  
  .swatch { 
    display: inline-flex; 
    align-items: center; 
    justify-content: center; 
    width: 1rem; 
    height: 1rem; 
    border-radius: .2rem; 
  }
  
  @media (max-width: 650px) {
    .calendar { gap: .3rem; }
    .day { min-height: 3.5rem; padding: .3rem; }
    .today-label { left: .15rem; font-size: .55rem; }
  }
</style>
