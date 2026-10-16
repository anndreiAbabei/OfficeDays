import { expect, it } from 'vitest';
import { reportCsv } from './reports';
import { reportFixture } from './reports.fixture';

it('exports the supplied snapshot including metadata and blank future attendance', () => {
  const csv = reportCsv(reportFixture);
  expect(csv).toContain('"Generated at (UTC)"');
  expect(csv).toContain('"Andrei","2026-09-15T09:00:00+00:00","UTC"');
  expect(csv).toContain('"20","50","10","5","25","Below target"');
  expect(csv).toContain('"22","70","16","","","Not started"');
  expect(csv.split('\r\n')).toHaveLength(5);
  expect(reportFixture.months[2].officeDays).toBeNull();
});

it('escapes quotes, commas and line breaks and neutralizes spreadsheet formula usernames', () => {
  const csv = reportCsv({ ...reportFixture, username: '=HYPERLINK("a,b")\nnext' });
  expect(csv).toContain('"\'=HYPERLINK(""a,b"")\nnext"');
  expect(csv.startsWith('\uFEFF')).toBe(true);
});

it('exports a header for an empty report without inventing monthly rows', () => {
  expect(reportCsv({ ...reportFixture, months: [] }).split('\r\n')).toHaveLength(2);
});
