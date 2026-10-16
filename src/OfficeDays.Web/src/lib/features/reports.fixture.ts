import type { AttendanceReport } from './reports';

export const reportFixture: AttendanceReport = {
  year: new Date().getUTCFullYear(), startMonth: 1, endMonth: 12, username: 'Andrei', timeZoneId: 'UTC',
  registrationMonth: '2026-06-01', today: '2026-09-15', generatedAt: '2026-09-15T09:00:00+00:00',
  summary: { officeDays: 8, attendancePercentage: 20, completedMonths: 1, monthsMeetingTarget: 0 },
  months: [
    { month: 6, eligibleWorkingDays: 20, requiredOfficePercentage: 50, requiredOfficeDays: 10, officeDays: 5, attendancePercentage: 25, result: 'belowTarget' },
    { month: 9, eligibleWorkingDays: 20, requiredOfficePercentage: 60, requiredOfficeDays: 12, officeDays: 3, attendancePercentage: 15, result: 'inProgress' },
    { month: 12, eligibleWorkingDays: 22, requiredOfficePercentage: 70, requiredOfficeDays: 16, officeDays: null, attendancePercentage: null, result: 'notStarted' }
  ]
};

