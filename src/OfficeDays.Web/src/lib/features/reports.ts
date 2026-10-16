export type ReportResult = 'notStarted' | 'inProgress' | 'targetMet' | 'belowTarget';

export interface ReportMonth {
  month: number;
  eligibleWorkingDays: number;
  requiredOfficePercentage: number;
  requiredOfficeDays: number;
  officeDays: number | null;
  attendancePercentage: number | null;
  result: ReportResult;
}

export interface AttendanceReport {
  year: number;
  startMonth: number;
  endMonth: number;
  username: string;
  timeZoneId: string;
  registrationMonth: string;
  today: string;
  generatedAt: string;
  summary: { officeDays: number; attendancePercentage: number | null; completedMonths: number; monthsMeetingTarget: number };
  months: ReportMonth[];
}

export const resultLabels: Record<ReportResult, string> = {
  notStarted: 'Not started', inProgress: 'In progress', targetMet: 'Target met', belowTarget: 'Below target'
};

function csvCell(value: string | number | null): string {
  let text = value === null ? '' : String(value);
  // Quoting handles CSV syntax; the apostrophe prevents user-controlled text becoming a spreadsheet formula.
  if (typeof value === 'string' && /^[\s]*[=+\-@\t\r\n]/.test(text)) text = `'${text}`;
  return `"${text.replaceAll('"', '""')}"`;
}

export function reportCsv(report: AttendanceReport): string {
  const header = ['Username', 'Generated at (UTC)', 'Timezone', 'Month', 'Eligible working days', 'Required office percentage',
    'Required office days', 'Recorded office days', 'Attendance percentage', 'Result'];
  const rows = report.months.map(month => [report.username, report.generatedAt, report.timeZoneId,
    `${String(report.year).padStart(4, '0')}-${String(month.month).padStart(2, '0')}`,
    month.eligibleWorkingDays, month.requiredOfficePercentage, month.requiredOfficeDays, month.officeDays,
    month.attendancePercentage, resultLabels[month.result]]);
  return '\uFEFF' + [header, ...rows].map(row => row.map(csvCell).join(',')).join('\r\n') + '\r\n';
}

export function downloadReport(report: AttendanceReport): void {
  const url = URL.createObjectURL(new Blob([reportCsv(report)], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = `office-days-${report.year}-${String(report.startMonth).padStart(2, '0')}-to-${String(report.endMonth).padStart(2, '0')}.csv`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
