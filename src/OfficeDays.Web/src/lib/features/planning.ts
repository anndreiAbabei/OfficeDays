import type { PlanningDay } from '$lib/types';

export function describeDay(day: PlanningDay): string {
  const details = [day.isToday ? 'Today' : '', day.isAttended ? 'Office attendance recorded' : '',
    ...day.holidays, day.isVacation ? 'Vacation' : '',
    day.isPlanned ? day.status === 'missed' ? 'Planned · no attendance recorded' : 'Planned office day' : '',
    !day.isEligible && !day.isVacation && !day.holidays.length ? 'Weekend' : ''];
  return details.filter(Boolean).join(' · ') || 'No office plan';
}

export function togglePlan(day: PlanningDay): PlanningDay {
  const isPlanned = !day.isPlanned;
  return { ...day, isPlanned, status: day.isAttended ? 'attended' : isPlanned ? 'planned' : 'none' };
}
