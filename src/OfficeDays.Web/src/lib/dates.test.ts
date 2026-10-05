import { describe, expect, it } from 'vitest';
import { localDate, shiftPeriod, isoDate } from './dates';
describe('calendar dates', () => {
  it('uses the user timezone across midnight instead of the device date', () => {
    expect(localDate('Europe/Bucharest', new Date('2026-09-30T22:30:00Z'))).toEqual({ year: 2026, month: 10, day: 1 });
    expect(localDate('America/Los_Angeles', new Date('2026-09-30T22:30:00Z'))).toEqual({ year: 2026, month: 9, day: 30 });
  });
  it('rolls between years and clamps supported date boundaries', () => {
    expect(shiftPeriod({ year: 2026, month: 1 }, -1)).toEqual({ year: 2025, month: 12 });
    expect(shiftPeriod({ year: 2026, month: 12 }, 1)).toEqual({ year: 2027, month: 1 });
    expect(shiftPeriod({ year: 1, month: 1 }, -1)).toEqual({ year: 1, month: 1 });
    expect(shiftPeriod({ year: 9999, month: 12 }, 1)).toEqual({ year: 9999, month: 12 });
    expect(isoDate(1, 2, 3)).toBe('0001-02-03');
  });
});
