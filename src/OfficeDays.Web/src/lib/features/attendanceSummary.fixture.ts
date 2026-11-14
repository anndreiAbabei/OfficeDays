import type { AttendanceStatus } from './attendanceSummary';

export const attendanceStatusFixture: AttendanceStatus = {
  period: '2026-10', eligibleWorkingDays: 20, maximumWfhDays: 10, requiredOfficeDays: 10,
  officeDays: 8, remainingOfficeDays: 2, progressPercentage: 80, upcomingPlannedDays: 1,
  summary: {
    today: '2026-10-30', totalDays: 31, workingDays: 22, bankHolidayWorkingDays: 1, vacationWorkingDays: 1,
    requirementMode: 'Percentage', requirementValue: 50, requiredOfficeDaysWithoutAbsences: 11,
    plannedDays: 3, remainingEligibleDays: 1, outstandingOfficeDays: 1,
    bankHolidays: [{ date: '2026-10-01', name: 'Weekday holiday' }, { date: '2026-10-03', name: 'Weekend holiday' }],
    vacationDates: ['2026-10-01', '2026-10-02']
  }
};
