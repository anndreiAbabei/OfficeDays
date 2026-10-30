import type { AttendanceRequirementMode } from '$lib/attendanceRequirementMode';
import type { TeamDay } from '$lib/types';

export const guestStorageKey = 'officedays.guest.v1';
export type GuestRequirement = { id: string; startingFrom: string; requirementMode: AttendanceRequirementMode; requirementValue: number; createdAt: string };
export type GuestData = {
  version: 1; 
  id: string; 
  createdAt: string; 
  registrationMonth: string;
  countryCode: string; 
  countryName: string; 
  timeZoneId: string; 
  teamDay: TeamDay | null;
  requirements: GuestRequirement[];
  attendance: { id: string; date: string; isManual: boolean; createdAt: string }[];
  plans: { id: string; date: string; createdAt: string }[];
  vacations: { id: string; from: string; to: string; createdAt: string }[];
  holidays: Record<string, { date: string; name: string }[]>;
};

export function validDate(value: unknown): value is string {
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(value) || value < '0001-01-01') 
    return false;
  
  const date = new Date(`${value}T00:00:00Z`);
  
  return Number.isFinite(date.getTime()) && date.toISOString().slice(0, 10) === value;
}
export function validRequirement(mode: unknown, value: unknown): boolean {
  return (mode === 'Percentage' || mode === 'DaysPerWeek') && 
      typeof value === 'number' && 
      Number.isInteger(value) && 
      value >= 0 && 
      value <= (mode === 'Percentage' ? 100 : 5);
}

function record(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function rows(value: unknown, check: (row: Record<string, unknown>) => boolean): boolean {
  return Array.isArray(value) && 
         value.every((row: unknown): boolean => record(row) && check(row));
}

export function validateGuestData(value: unknown): value is GuestData {
  if (!record(value) || value.version !== 1 || typeof value.id !== 'string' || typeof value.createdAt !== 'string'
    || !validDate(value.registrationMonth) || !value.registrationMonth.endsWith('-01')
    || typeof value.countryCode !== 'string' || !value.countryCode || typeof value.countryName !== 'string'
    || typeof value.timeZoneId !== 'string'
    || ![null, 'monday', 'tuesday', 'wednesday', 'thursday', 'friday'].includes(value.teamDay as string | null)) 
    return false;
  
  try { 
    new Intl.DateTimeFormat('en', { timeZone: value.timeZoneId }); 
  } catch { 
    return false; 
  }
  
  const entity = (r: Record<string, unknown>): boolean => typeof r.id === 'string' && typeof r.createdAt === 'string';
  
  return rows(value.requirements, (r): boolean => entity(r) && validDate(r.startingFrom) && r.startingFrom.endsWith('-01') && validRequirement(r.requirementMode, r.requirementValue))
    && (value.requirements as GuestRequirement[]).filter((r): boolean => r.startingFrom === '0001-01-01').length === 1
    && rows(value.attendance, (r): boolean => entity(r) && validDate(r.date) && typeof r.isManual === 'boolean')
    && rows(value.plans, (r): boolean => entity(r) && validDate(r.date))
    && rows(value.vacations, (r): boolean => entity(r) && validDate(r.from) && validDate(r.to) && r.from <= r.to)
    && record(value.holidays) && Object.entries(value.holidays).every(([year, holidays]): boolean => /^\d{4}$/.test(year)
      && rows(holidays, (r): boolean => validDate(r.date) && r.date.startsWith(year) && typeof r.name === 'string'));
}
export function readGuestData(storage: Storage = localStorage): GuestData | null {
  const raw = storage.getItem(guestStorageKey);
  
  if (raw === null) 
    return null;
  
  let value: unknown;
  
  try {
    value = JSON.parse(raw); 
  } catch { 
    throw new Error('Saved guest data could not be read. It has been preserved; do not clear browser data.'); 
  }
  
  if (!validateGuestData(value)) 
    throw new Error('Saved guest data is invalid or uses an unsupported version. It has been preserved.');
  
  return value;
}
export function saveGuestData(data: GuestData, storage: Storage = localStorage): void {
  if (!validateGuestData(data)) 
    throw new Error('Guest data is invalid; changes were not saved.');
  
  try { 
    storage.setItem(guestStorageKey, JSON.stringify(data)); 
  }
  catch { 
    throw new Error('This browser could not save your data. Changes were not saved. Check available storage and browser permissions.'); 
  }
}
