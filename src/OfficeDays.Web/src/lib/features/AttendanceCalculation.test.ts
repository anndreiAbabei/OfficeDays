import { render, screen, within } from '@testing-library/svelte';
import { expect, it } from 'vitest';
import AttendanceCalculation from './AttendanceCalculation.svelte';
import { attendanceStatusFixture } from './attendanceSummary.fixture';

it('Should_ShowEveryTotalAndExplainAbsences_WhenRenderingCalculation', (): void => {
  // Arrange
  const expected = new Map([
    ['Total days', '31 including weekends'], ['Working days', '22 Monday–Friday'],
    ['Bank holidays', '2 days · 1 weekday excluded'], ['Vacation', '2 days · 1 additional weekday excluded'],
    ['Attendance requirement', '50%'], ['Required office days', '10 days'],
    ['Planned days this month', '3 days'], ['Upcoming planned days', '1 day'],
    ['Office days recorded', '8 days'], ['Office days left to attend', '2 days'],
    ['Available days left', '1 day'], ['Unavoidable outstanding days', '1 day']
  ]);

  // Act
  render(AttendanceCalculation, { status: attendanceStatusFixture, upcomingPlanned: 1 });

  // Assert
  for (const [label, value] of expected)
    expect(screen.getByText(label, { selector: 'dt' }).nextElementSibling).toHaveTextContent(value);
  expect(screen.getByRole('list', { name: 'Bank holiday dates' })).toHaveTextContent('Weekend; no deduction');
  expect(screen.getByRole('list', { name: 'Vacation dates' })).toHaveTextContent('Bank holiday; already excluded');
  expect(screen.getByRole('region', { name: 'Office target calculation' })).toHaveTextContent('20 eligible working days × 50% = 10 days, rounded up');
  expect(screen.getByText(/As of/)).toHaveTextContent('in your timezone');
});

it('Should_ShowWeeklyEquivalentAndEmptyAbsences_WhenRequirementUsesDaysPerWeek', (): void => {
  // Arrange
  const status = { ...attendanceStatusFixture, eligibleWorkingDays: 22, requiredOfficeDays: 14,
    summary: { ...attendanceStatusFixture.summary, requirementMode: 'DaysPerWeek' as const, requirementValue: 3,
      bankHolidays: [], vacationDates: [], bankHolidayWorkingDays: 0, vacationWorkingDays: 0 } };
  const expectedRequirement = '3 days per week (60%)';

  // Act
  render(AttendanceCalculation, { status, upcomingPlanned: 0 });

  // Assert
  expect(screen.getByText('Attendance requirement').nextElementSibling).toHaveTextContent(expectedRequirement);
  expect(screen.getByText('No bank holidays this month.')).toBeVisible();
  expect(screen.getByText('No vacation this month.')).toBeVisible();
  expect(screen.queryByRole('list')).not.toBeInTheDocument();
  expect(screen.queryByText(/Without holidays or vacation/)).not.toBeInTheDocument();
  expect(screen.getByText(/Each day per week/)).toHaveTextContent('rounded once for the month');
});

it.each([0, 2])('Should_ShowCapacityWithoutWarning_WhenOutstandingIsZeroAndRemainingIs%i', (remaining): void => {
  // Arrange
  const status = { ...attendanceStatusFixture, remainingOfficeDays: remaining,
    summary: { ...attendanceStatusFixture.summary, outstandingOfficeDays: 0, remainingEligibleDays: 3 } };
  const expectedMessage = remaining === 0 ? 'Your office requirement is met.' : 'There are enough eligible days left to meet your target.';

  // Act
  render(AttendanceCalculation, { status, upcomingPlanned: 0 });

  // Assert
  expect(screen.getByText(expectedMessage)).toBeVisible();
  expect(screen.queryByText(/that cannot be covered/)).not.toBeInTheDocument();
  const target = within(screen.getByRole('region', { name: 'Office target calculation' }));
  expect(target.getByText('Unavoidable outstanding days').nextElementSibling).toHaveTextContent('0 days');
});
