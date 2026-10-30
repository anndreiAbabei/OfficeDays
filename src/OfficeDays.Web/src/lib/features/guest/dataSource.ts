import type { DataSource } from '../dataSource';
import type { GuestData, GuestRequirement } from './storage';
import { readGuestData, saveGuestData, validDate, validRequirement } from './storage';
import { planning, report, requirementAt, status, todayIn } from './calculations';
import { isoDate, localDate } from '$lib/dates';

export function createGuestSource(storage: Storage = localStorage, clock: () => Date = (): Date => new Date()): DataSource {
  
  async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const data = readGuestData(storage);
    
    if (!data) 
      throw new Error('Complete guest setup first.');
    
    // This adapter implements only local operations. Unknown paths never reach fetch.
    return execute(data, path, options, clock(), storage) as T;
  }
  
  async function mutate<T>(path: string, method: string, body?: unknown): Promise<T> {
    return request<T>(path, { method, body: body === undefined ? undefined : JSON.stringify(body) });
  }
  
  return { request, mutate };
}
function execute(data: GuestData, path: string, options: RequestInit, now: Date, storage: Storage): unknown {
  const url = new URL(path, 'https://local.invalid');
  const route = url.pathname;
  const method = options.method ?? 'GET';
  const today = todayIn(data, now);
  const current = localDate(data.timeZoneId, now);
  const year = Number(url.searchParams.get('year') ?? current.year);
  const month = Number(url.searchParams.get('month') ?? current.month);
  
  if (!Number.isInteger(year) || year < 1 || year > 9998 || !Number.isInteger(month) || month < 1 || month > 12) 
    throw new Error('Choose a valid period.');
  
  const start = isoDate(year, month);
  const end = `${start.slice(0, 7)}-31`;
  
  if (method === 'GET') {
    if (route === '/api/status') return status(data, { year, month }, now);
    if (route === '/api/planning') return planning(data, { year, month }, now);
    if (route === '/api/attendance') return data.attendance.filter((a): boolean => a.date >= start && a.date <= end).sort((a, b): number => b.date.localeCompare(a.date));
    if (route === '/api/vacations') return data.vacations.filter((v): boolean => v.from <= end && v.to >= start).sort((a, b): number => a.from.localeCompare(b.from));
    if (route === '/api/users/me/requirements') return { today, items: data.requirements.map((r): GuestRequirement & { isBaseline: boolean } => ({ ...r, isBaseline: r.startingFrom === '0001-01-01' })) };
    if (route === '/api/auth/me') return { ...requirementAt(data, `${today.slice(0, 7)}-01`), id: data.id, username: 'Guest', email: null, timeZoneId: data.timeZoneId, countryCode: data.countryCode, countryName: data.countryName, isAdmin: false, teamDay: data.teamDay };
    if (route === '/api/reports') {
      const first = Number(url.searchParams.get('startMonth') ?? 1);
      const last = Number(url.searchParams.get('endMonth') ?? 12);
      
      if (![first, last].every((m): boolean => Number.isInteger(m) && m >= 1 && m <= 12) || first > last) 
        throw new Error('Choose a valid month range.');
      
      return report(data, year, first, last, now);
    }
    throw new Error('This feature requires a full account.');
  }
  
  const body: Record<string, unknown> = typeof options.body === 'string' 
                    ? JSON.parse(options.body) 
                    : {};
  
  const date = route.split('/').at(-1) ?? '';
  const entity = (): { id: string; createdAt: string } => ({ id: crypto.randomUUID(), createdAt: now.toISOString() });
  
  if (/^\/api\/(attendance|planning)\//.test(route) && ['PUT', 'DELETE'].includes(method)) {
    if (!validDate(date) || date < data.registrationMonth) 
      throw new Error('Choose a date on or after your first tracking month.');
    
    if (route.startsWith('/api/attendance/')) {
      if (date > today) 
        throw new Error('Attendance cannot be recorded in the future.');
      
      data.attendance = method === 'DELETE' 
          ? data.attendance.filter((a): boolean => a.date !== date) 
          : data.attendance.some((a): boolean => a.date === date)
              ? data.attendance 
              : [...data.attendance, { ...entity(), date, isManual: true }];
    } else {
      if (date < today) 
        throw new Error('Past plans cannot be changed.');
      
      const period = {year: Number(date.slice(0, 4)), month: Number(date.slice(5, 7)) };
      const day = planning(data, period, now).days.find((d): boolean => d.date === date);
      
      if (method === 'PUT' && !day?.isEligible) 
        throw new Error('Plans must be on eligible working days.');
      
      data.plans = method === 'DELETE' 
          ? data.plans.filter((p): boolean => p.date !== date) 
          : data.plans.some((p): boolean => p.date === date) 
              ? data.plans 
              : [...data.plans, { ...entity(), date }];
    }
  } else if ((route === '/api/vacations' && method === 'POST') || 
             (route.startsWith('/api/vacations/') && ['PUT', 'DELETE'].includes(method))) {
    
    const existing = data.vacations.find((v): boolean => v.id === date);
    
    if (method === 'DELETE')
      data.vacations = data.vacations.filter((v): boolean => v.id !== date);
    else {
      if (!validDate(body.from) || !validDate(body.to) || body.from > body.to || body.from < data.registrationMonth) 
        throw new Error('Choose a valid vacation range on or after your first tracking month.');
      
      if (method === 'PUT' && !existing) 
        throw new Error('Vacation not found.');
      
      data.vacations = [
          ...data.vacations.filter((v): boolean => v.id !== date), 
          { ...(existing ?? entity()), from: body.from, to: body.to }
      ];
    }
  } else if (route.startsWith('/api/users/me/requirements') && ['POST', 'PUT', 'DELETE'].includes(method)) {
    const existing = data.requirements.find((r): boolean => r.id === date);
    
    if (method !== 'POST' && !existing) 
      throw new Error('Requirement not found.');
    
    if (method === 'DELETE') {
      if (existing?.startingFrom === '0001-01-01') 
        throw new Error('The initial requirement cannot be deleted.');
      
      data.requirements = data.requirements.filter((r): boolean => r.id !== date);
    } else {
      if (!validDate(body.startingFrom) || !body.startingFrom.endsWith('-01') || !validRequirement(body.requirementMode, body.requirementValue)) 
        throw new Error('Choose a valid requirement and effective month.');
      
      if (existing?.startingFrom === '0001-01-01' && body.startingFrom !== existing.startingFrom) 
        throw new Error('The initial requirement date cannot change.');
      
      if (data.requirements.some((r): boolean => r.id !== existing?.id && r.startingFrom === body.startingFrom)) 
        throw new Error('A requirement already exists for this month.');
      
      data.requirements = [ 
          ...data.requirements.filter((r): boolean => r.id !== date), 
          { ...(existing ?? entity()), 
            startingFrom: body.startingFrom, 
            requirementMode: body.requirementMode as 'Percentage' | 'DaysPerWeek', 
            requirementValue: body.requirementValue as number 
          }
      ];
    }
  } else 
    throw new Error('This feature requires a full account.');
  
  saveGuestData(data, storage);
  return undefined;
}
