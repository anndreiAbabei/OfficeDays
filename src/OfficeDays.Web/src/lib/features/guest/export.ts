import { localDate, isoDate } from '$lib/dates';
import type { GuestData } from './storage';
import { todayIn } from './calculations';

export function guestExport(data: GuestData, now: Date): object {
  const registered = localDate(data.timeZoneId, new Date(data.createdAt));
  
  return {
    format: 'officedays-guest-data', formatVersion: 1, exportedAt: now.toISOString(),
    application: { name: 'OfficeDays' },
    reportContext: { calculationVersion: 1, localDate: todayIn(data, now), registrationLocalDate: isoDate(registered.year, registered.month, registered.day) },
    profile: { id: data.id, username: 'Guest', timeZoneId: data.timeZoneId, countryCode: data.countryCode, createdAt: data.createdAt },
    preferences: { teamDay: data.teamDay },
    requirementHistory: data.requirements.map((r): GuestData['requirements'][number] & { userId: string } => ({ ...r, userId: data.id })),
    attendance: data.attendance.map((a): GuestData['attendance'][number] & { userId: string } => ({ ...a, userId: data.id })),
    plans: data.plans.map((p): GuestData['plans'][number] & { userId: string } => ({ ...p, userId: data.id })),
    vacations: data.vacations.map((v): GuestData['vacations'][number] & { userId: string } => ({ ...v, userId: data.id })),
    holidayCalendar: { code: data.countryCode, name: data.countryName, downloadedYears: Object.keys(data.holidays).map(Number).sort((a, b): number => a - b), holidays: Object.values(data.holidays).flat() }
  };
}

export function downloadGuestData(data: GuestData): void {
  const url = URL.createObjectURL(new Blob([JSON.stringify(guestExport(data, new Date()), null, 2)], { type: 'application/json' }));
  const link = document.createElement('a');
  
  link.href = url; link.download = 'officedays-guest-data.json'; document.body.appendChild(link);
  
  try { 
    link.click(); 
  }
  finally { 
    link.remove(); 
    setTimeout(() => URL.revokeObjectURL(url), 1000); 
  }
}
