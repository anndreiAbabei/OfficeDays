import type { Period } from './types';
export const months = Array.from({ length: 12 }, (_, month) => new Intl.DateTimeFormat(undefined, { month: 'long' }).format(new Date(2024, month, 1)));

export function localDate(timeZone: string, now = new Date()): {year: number, month: number, day: number} {
  const parts = new Intl.DateTimeFormat('en', { timeZone, year: 'numeric', month: 'numeric', day: 'numeric' }).formatToParts(now);
  const part = (type: string) => Number(parts.find(p => p.type === type)!.value);
  
  return { year: part('year'), month: part('month'), day: part('day') };
}

export function isoDate(year: number, month: number, day = 1): string {
  return `${String(year).padStart(4, '0')}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

export function shiftPeriod(period: Period, delta: number): Period {
  const index = (period.year - 1) * 12 + period.month - 1 + delta;
  const bounded = Math.max(0, Math.min(9999 * 12 - 1, index));
  
  return { year: Math.floor(bounded / 12) + 1, month: bounded % 12 + 1 };
}
export const periodQuery = (p: Period) => `year=${p.year}&month=${p.month}`;
export const displayDate = (value: string) => new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeZone: 'UTC' }).format(new Date(`${value}T00:00:00Z`));
export const displayTime = (value: string | null) => value ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) : 'Never';
