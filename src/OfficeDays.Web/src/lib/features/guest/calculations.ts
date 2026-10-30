import { isoDate, localDate } from '$lib/dates';
import type { Period, PlanningMonth, Status } from '$lib/types';
import type { AttendanceReport, ReportMonth } from '../reports';
import type { GuestData, GuestRequirement } from './storage';

export function todayIn(data: GuestData, now: Date): string {
  const date = localDate(data.timeZoneId, now);
  
  return isoDate(date.year, date.month, date.day);
}
export function requirementAt(data: GuestData, month: string): GuestRequirement {
  const result = [...data.requirements].sort((a, b): number => b.startingFrom.localeCompare(a.startingFrom))
                                       .find((r): boolean => r.startingFrom <= month);
  
  if (!result) 
    throw new Error('Initial attendance requirement is missing.');
  
  return result;
}
// Match decimal Math.Round(..., 1) on the server, including midpoint-to-even.
export function percentage(numerator: number, denominator: number): number {
  const scaled = numerator * 1000;
  const quotient = Math.floor(scaled / denominator);
  const remainder = scaled % denominator;
  
  return (quotient + 
            (remainder * 2 > denominator || (remainder * 2 === denominator && quotient % 2 !== 0)  
                ? 1 : 0)
         ) / 10;
}
export function planning(data: GuestData, period: Period, now: Date): PlanningMonth {
  const today = todayIn(data, now);
  const holidays = data.holidays[String(period.year).padStart(4, '0')] ?? [];
  const days: PlanningMonth['days'] = [];
  
  for (let day = 1; day <= 31; day++) {
    const date = isoDate(period.year, period.month, day);
    const parsed = new Date(`${date}T00:00:00Z`);
    
    if (parsed.getUTCMonth() + 1 !== period.month) 
      break;
    
    const names = holidays.filter((h): boolean => h.date === date)
                          .map((h): string => h.name);
    
    const isVacation = data.vacations.some((v): boolean => v.from <= date && date <= v.to);
    const isEligible = ![0, 6].includes(parsed.getUTCDay()) && !names.length && !isVacation;
    const isAttended = data.attendance.some((a): boolean => a.date === date);
    const isPlanned = data.plans.some((p): boolean => p.date === date);
    
    days.push({ 
      date, 
      isVacation, 
      isEligible,
      isAttended, 
      isPlanned, 
      isToday: date === today,
      canToggle: date >= today && isEligible && date >= data.registrationMonth,
      holidays: names, 
      status: isAttended 
              ? 'attended' 
              : isVacation || names.length 
                  ? 'holiday' 
                  : isPlanned 
                      ? date < today 
                          ? 'missed' 
                          : 'planned' 
                      : 'none' });
  }
  return { 
    today, 
    days, 
    upcomingPlannedDays: days.filter((d): boolean => d.date >= today && d.isPlanned && d.isEligible && !d.isAttended)
                             .length 
  };
}
export function status(data: GuestData, period: Period, now: Date): Status {
  const calendar = planning(data, period, now);
  const eligibleWorkingDays = calendar.days.filter((d): boolean => d.isEligible).length;
  const requirement = requirementAt(data, isoDate(period.year, period.month));
  const percent = requirement.requirementValue * (requirement.requirementMode === 'DaysPerWeek' ? 20 : 1);
  const requiredOfficeDays = Math.ceil(eligibleWorkingDays * percent / 100);
  const officeDays = calendar.days.filter((d): boolean => d.isEligible && d.isAttended && d.date <= calendar.today).length;
  
  return { 
    period: isoDate(period.year, period.month).slice(0, 7), 
    eligibleWorkingDays, 
    requiredOfficeDays, 
    officeDays,
    maximumWfhDays: eligibleWorkingDays - requiredOfficeDays, 
    remainingOfficeDays: Math.max(0, requiredOfficeDays - officeDays),
    progressPercentage: requiredOfficeDays === 0 ? 100 : Math.min(100, percentage(officeDays, requiredOfficeDays)), 
    upcomingPlannedDays: calendar.upcomingPlannedDays
  };
}
export function report(data: GuestData, year: number, startMonth: number, endMonth: number, now: Date): AttendanceReport {
  const today = todayIn(data, now);
  const months: ReportMonth[] = [];
  
  for (let month = startMonth; month <= endMonth; month++) {
    const start = isoDate(year, month);
    
    if (start < data.registrationMonth) 
      continue;
    
    const summary = status(data, { year, month }, now);
    const requirement = requirementAt(data, start);
    const future = start.slice(0, 7) > today.slice(0, 7);
    
    months.push({ 
      month, 
      eligibleWorkingDays: summary.eligibleWorkingDays,
      requirementMode: requirement.requirementMode, 
      requirementValue: requirement.requirementValue,
      requiredOfficeDays: summary.requiredOfficeDays, 
      officeDays: future ? null : summary.officeDays,
      attendancePercentage: future || !summary.eligibleWorkingDays 
                                ? null : percentage(summary.officeDays, summary.eligibleWorkingDays),
      result: future 
                ? 'notStarted' 
                : start.slice(0, 7) === today.slice(0, 7) 
                    ? 'inProgress' 
                    : summary.officeDays >= summary.requiredOfficeDays 
                        ? 'targetMet' : 'belowTarget' 
    });
  }
  const started = months.filter((m): boolean => m.result !== 'notStarted');
  const completed = months.filter((m): boolean => m.result === 'targetMet' || m.result === 'belowTarget');
  const officeDays = started.reduce((sum, m): number => sum + (m.officeDays ?? 0), 0);
  const eligible = started.reduce((sum, m): number => sum + m.eligibleWorkingDays, 0);
  
  return { 
    year, 
    startMonth, 
    endMonth, 
    username: 'Guest', 
    timeZoneId: data.timeZoneId, 
    registrationMonth: 
    data.registrationMonth,
    today, generatedAt: now.toISOString(), 
    months,
    summary: { 
      officeDays, 
      attendancePercentage: eligible 
          ? percentage(officeDays, eligible) 
          : null,
      completedMonths: completed.length, 
      monthsMeetingTarget: completed.filter((m): boolean => m.result === 'targetMet').length 
    } 
  };
}
