import { render, screen } from '@testing-library/svelte';
import { expect, it } from 'vitest';
import StatusSummary from './StatusSummary.svelte';

it('shows the API summary and bounds the accessible progress bar', () => {
  render(StatusSummary, { status: { period: '2026-10', eligibleWorkingDays: 22, maximumWfhDays: 11, requiredOfficeDays: 11, officeDays: 14, remainingOfficeDays: 0, upcomingPlannedDays: 3, progressPercentage: 127.27 } });
  
  expect(screen.getByText('14')).toBeInTheDocument();
  expect(screen.getByText('0')).toBeInTheDocument();
  expect(screen.getByText('127.27%')).toBeInTheDocument();
  expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '100');
});
it('shows an indeterminate loading state without misleading zero totals', () => {
  render(StatusSummary, { loading: true });
  
  expect(screen.getByLabelText('Monthly status')).toHaveAttribute('aria-busy', 'true');
  expect(screen.getByRole('progressbar')).not.toHaveAttribute('aria-valuenow');
  expect(screen.queryByText('0')).not.toBeInTheDocument();
});

it.each([
  [5, 8, '3 more days to plan.'],
  [7, 8, '1 more day to plan.'],
  [8, 8, 'All remaining days planned.'],
  [9, 8, 'All remaining days planned.'],
  [0, 0, 'Office requirement met.']
])('shows %i upcoming plans against %i remaining office days', (planned, remaining, message) => {
  render(StatusSummary, { status: { period: '2026-10', eligibleWorkingDays: 22, maximumWfhDays: 11,
    requiredOfficeDays: 11, officeDays: 11 - remaining, remainingOfficeDays: remaining,
    upcomingPlannedDays: planned, progressPercentage: 0 } });

  expect(screen.getByText(`${planned} / ${remaining}`)).toBeInTheDocument();
  expect(screen.getByText(message)).toBeInTheDocument();
});

it('uses the instant calendar count while waiting for the refreshed summary', () => {
  render(StatusSummary, { plannedCount: 6, status: { period: '2026-10', eligibleWorkingDays: 22,
    maximumWfhDays: 11, requiredOfficeDays: 11, officeDays: 3, remainingOfficeDays: 8,
    upcomingPlannedDays: 5, progressPercentage: 27.3 } });

  expect(screen.getByText('6 / 8')).toBeInTheDocument();
  expect(screen.getByText('2 more days to plan.')).toBeInTheDocument();
});
