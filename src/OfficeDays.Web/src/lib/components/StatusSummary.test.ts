import { fireEvent, render, screen, within } from '@testing-library/svelte';
import { expect, it } from 'vitest';
import StatusSummary from './StatusSummary.svelte';
import { attendanceStatusFixture } from '$lib/features/attendanceSummary.fixture';

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

it('Should_ExpandAndCollapseCalculation_WhenButtonIsActivated', async (): Promise<void> => {
  // Arrange
  render(StatusSummary, { status: attendanceStatusFixture });
  const toggle = screen.getByRole('button', { name: 'Details' });
  const expectedCollapsed = 'false';
  const expectedExpanded = 'true';
  expect(toggle).toHaveAttribute('aria-expanded', expectedCollapsed);
  expect(screen.queryByRole('region', { name: 'Calendar calculation' })).not.toBeInTheDocument();

  // Act
  await fireEvent.click(toggle);

  // Assert
  expect(toggle).toHaveAttribute('aria-expanded', expectedExpanded);
  expect(toggle).toHaveAccessibleName('Details');
  const calendar = screen.getByRole('region', { name: 'Calendar calculation' });
  expect(calendar).toBeVisible();
  expect(within(calendar).getByRole('list', { name: 'Bank holiday dates' })).toHaveTextContent('Weekday holiday');
  expect(within(calendar).getByRole('list', { name: 'Vacation dates' })).toHaveTextContent('Bank holiday; already excluded');
  expect(calendar).toHaveTextContent('20 eligible working days');
  expect(screen.getByText(/Without holidays or vacation/)).toHaveTextContent('11 office days out of 22 weekdays');
  expect(screen.getByText(/that cannot be covered on eligible working days/)).toHaveTextContent('1 day that cannot be covered on eligible working days');

  // Act
  await fireEvent.click(toggle);

  // Assert
  expect(toggle).toHaveAttribute('aria-expanded', expectedCollapsed);
  expect(screen.queryByRole('region', { name: 'Calendar calculation' })).not.toBeInTheDocument();
});

it('Should_DisableCalculation_WhenSummaryIsLoadingOrUnavailable', async (): Promise<void> => {
  // Arrange
  const { rerender } = render(StatusSummary, { loading: true });
  const toggle = screen.getByRole('button', { name: 'Details' });

  // Act / Assert
  expect(toggle).toBeDisabled();
  await rerender({ status: attendanceStatusFixture, loading: true });
  expect(toggle).toBeDisabled();
  await rerender({ status: attendanceStatusFixture, loading: false });
  expect(toggle).toBeEnabled();
});

it('Should_UpdateExpandedCalculation_WhenFreshSummaryArrives', async (): Promise<void> => {
  // Arrange
  const { rerender } = render(StatusSummary, { status: attendanceStatusFixture });
  await fireEvent.click(screen.getByRole('button', { name: 'Details' }));
  const expectedAvailable = '0 days';
  const updated = { ...attendanceStatusFixture, summary: { ...attendanceStatusFixture.summary, remainingEligibleDays: 0, outstandingOfficeDays: 2 } };

  // Act
  await rerender({ status: updated });

  // Assert
  expect(screen.getByRole('button', { name: 'Details' })).toHaveAttribute('aria-expanded', 'true');
  expect(screen.getByText('Available days left').nextElementSibling).toHaveTextContent(expectedAvailable);
  expect(screen.getByText(/that cannot be covered on eligible working days/)).toHaveTextContent('2 days that cannot be covered on eligible working days');
});

it.each([
  [9, 10, '1 day still available to plan.'],
  [10, 10, 'No more days available to plan.'],
  [0, 0, 'No more days available to plan.']
] as const)('Should_ShowOnlyAvailableDays_WhenTargetExceedsCapacityWith%iPlansAnd%iDaysLeft', (planned, available, hint): void => {
  // Arrange
  const remaining = 15;
  const shortfall = remaining - available;
  const status = { ...attendanceStatusFixture, remainingOfficeDays: remaining,
    summary: { ...attendanceStatusFixture.summary, remainingEligibleDays: available, outstandingOfficeDays: shortfall } };
  const expected = `${hint} Attending every remaining eligible day would still leave you ${shortfall} days short.`;

  // Act
  render(StatusSummary, { status, plannedCount: planned });

  // Assert
  expect(screen.getByText(expected)).toBeVisible();
});
