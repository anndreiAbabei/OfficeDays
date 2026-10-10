export interface User {
  id: string; 
  username: string; 
  email: string | null; 
  timeZoneId: string;
  countryCode: string; 
  countryName: string; 
  isAdmin: boolean; 
  requiredOfficePercentage: number;
}

export interface Status {
  period: string; 
  eligibleWorkingDays: number; 
  maximumWfhDays: number;
  requiredOfficeDays: number; 
  officeDays: number; 
  remainingOfficeDays: number; 
  progressPercentage: number;
  upcomingPlannedDays: number;
}

export interface Attendance { 
  date: string; 
  isManual: boolean 
}

export interface Vacation { 
  id: string; 
  from: string; 
  to: string 
}

export interface Jurisdiction { 
  code: string; 
  name: string 
}

export interface Holiday { 
  date: string;
  name: string 
}

export interface ApiToken {
  id: string; 
  name: string; 
  createdAt: string; 
  lastUsedAt: string | null; 
  status: string;
}

export interface Period { 
  year: number; 
  month: number 
}

export interface PlanningDay {
  date: string;
  isPlanned: boolean;
  isEligible: boolean;
  isAttended: boolean;
  isToday: boolean;
  canToggle: boolean;
  status: 'attended' | 'holiday' | 'missed' | 'planned' | 'none';
  holidays: string[];
  isVacation: boolean;
}

export interface PlanningMonth {
  today: string;
  upcomingPlannedDays: number;
  days: PlanningDay[];
}
