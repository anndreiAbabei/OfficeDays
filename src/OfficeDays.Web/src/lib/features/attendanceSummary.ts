import type { AttendanceRequirementMode } from '$lib/attendanceRequirementMode';
import type { Status } from '$lib/types';

export type AttendanceSummary = {
  today: string;
  totalDays: number;
  workingDays: number;
  bankHolidayWorkingDays: number;
  vacationWorkingDays: number;
  requirementMode: AttendanceRequirementMode;
  requirementValue: number;
  requiredOfficeDaysWithoutAbsences: number;
  plannedDays: number;
  remainingEligibleDays: number;
  outstandingOfficeDays: number;
  bankHolidays: { date: string; name: string }[];
  vacationDates: string[];
};

export type AttendanceStatus = Status & { summary: AttendanceSummary };
