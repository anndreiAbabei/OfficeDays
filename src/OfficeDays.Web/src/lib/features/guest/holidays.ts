import { request } from '$lib/api';
import { validDate } from './storage';

export async function downloadHolidays(code: string, year: number): Promise<{ date: string; name: string }[]> {
  const value = await request<unknown>(`/api/bank-holidays/${encodeURIComponent(code)}/${year}`);
  
  const failure = !Array.isArray(value) || !value.every((row: unknown): boolean => typeof row === 'object' && row !== null && 'date' in row &&
      validDate(row.date) &&
      row.date.startsWith(String(year).padStart(4, '0')) && 'name' in row && typeof row.name === 'string');
  
  if (failure) {
    throw new Error('The server returned an invalid holiday calendar. Please retry.');
  }
  
  return value as { date: string; name: string }[];
}
