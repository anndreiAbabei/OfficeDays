import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { expect, it, vi } from 'vitest';
import Reports from './Reports.svelte';
import { reportFixture } from './reports.fixture';
import * as reports from './reports';

it('shows monthly states, excludes changing filters from export, and exports without another fetch', async () => {
  const fetcher = vi.fn(async (_url: string) => Response.json(reportFixture));
  const download = vi.spyOn(reports, 'downloadReport').mockImplementation(() => {});
  vi.stubGlobal('fetch', fetcher);
  render(Reports, { timeZone: 'UTC' });
  await screen.findByRole('table');
  expect(screen.getByRole('cell', { name: 'Not started' })).toBeVisible();
  expect(screen.getByRole('cell', { name: 'In progress' })).toBeVisible();
  expect(screen.getByRole('cell', { name: 'Below target' })).toBeVisible();
  await fireEvent.click(screen.getByRole('button', { name: 'Export CSV' }));
  expect(download).toHaveBeenCalledWith(reportFixture);
  expect(fetcher).toHaveBeenCalledTimes(1);
  await fireEvent.change(screen.getByLabelText('From month'), { target: { value: '6' } });
  expect(screen.getByRole('button', { name: 'Export CSV' })).toBeDisabled();
  await fireEvent.submit(screen.getByRole('button', { name: 'Generate report' }).closest('form')!);
  await waitFor(() => expect(fetcher).toHaveBeenCalledTimes(2));
  expect(fetcher.mock.calls[1][0]).toContain('startMonth=6&endMonth=12');
});

it('shows loading, prevents export before a response, and recovers from errors', async () => {
  let reject!: (reason: Error) => void;
  const fetcher = vi.fn().mockImplementationOnce(() => new Promise((_, failure) => { reject = failure; }))
    .mockResolvedValue(Response.json(reportFixture));
  vi.stubGlobal('fetch', fetcher);
  render(Reports, { timeZone: 'UTC' });
  await screen.findByText('Generating your report…');
  expect(screen.getByRole('button', { name: 'Export CSV' })).toBeDisabled();
  reject(new Error('Connection lost'));
  await screen.findByRole('alert');
  await fireEvent.submit(screen.getByRole('button', { name: 'Generate report' }).closest('form')!);
  await screen.findByRole('table');
  expect(screen.queryByRole('alert')).not.toBeInTheDocument();
});

it('handles pre-registration ranges and rejects reversed filters without fetching', async () => {
  const fetcher = vi.fn(async () => Response.json({ ...reportFixture, months: [] }));
  vi.stubGlobal('fetch', fetcher);
  render(Reports, { timeZone: 'UTC' });
  await screen.findByText('No months in this range');
  expect(screen.getByRole('button', { name: 'Export CSV' })).toBeDisabled();
  await fireEvent.change(screen.getByLabelText('From month'), { target: { value: '7' } });
  await fireEvent.change(screen.getByLabelText('To month'), { target: { value: '6' } });
  await fireEvent.submit(screen.getByRole('button', { name: 'Generate report' }).closest('form')!);
  await screen.findByRole('alert');
  expect(fetcher).toHaveBeenCalledTimes(1);
});

it('Should_DisplayWeeklyDays_WhenReportPreservesWeeklyRequirement', async () => {
  // Arrange
  const report = { ...reportFixture, months: [{ ...reportFixture.months[0], mode: 'DaysPerWeek', value: 3, requiredOfficePercentage: 60 }] };
  vi.stubGlobal('fetch', vi.fn(async (): Promise<Response> => Response.json(report)));

  // Act
  render(Reports, { timeZone: 'UTC' });

  // Assert
  expect(await screen.findByRole('cell', { name: '3 days per week' })).toBeVisible();
});
