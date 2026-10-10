import { render, screen } from '@testing-library/svelte';
import { expect, it } from 'vitest';
import StatusSummary from './StatusSummary.svelte';

it('shows the API summary and bounds the accessible progress bar', () => {
  render(StatusSummary, { status: { period: '2026-10', eligibleWorkingDays: 22, maximumWfhDays: 11, requiredOfficeDays: 11, officeDays: 14, remainingOfficeDays: 0, progressPercentage: 127.27 } });
  
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
