import { render, screen, fireEvent, waitFor } from '@testing-library/svelte';
import { beforeEach, expect, test, vi } from 'vitest';
import { get } from 'svelte/store';
import DeleteAccount from './DeleteAccount.svelte';
import { mutate } from '$lib/api';
import { sessionMessage, signedOut } from '$lib/session';

vi.mock('$lib/api', () => ({ mutate: vi.fn(), errorMessage: (error: unknown): string => String(error) }));
vi.mock('$lib/session', async () => {
  const { writable } = await import('svelte/store');
  return { signedOut: vi.fn(), sessionMessage: writable('') };
});

beforeEach((): void => {
  vi.clearAllMocks();
  // jsdom has no native dialog implementation; browser tests verify focus and Escape.
  HTMLDialogElement.prototype.showModal = function (): void { this.setAttribute('open', ''); };
  HTMLDialogElement.prototype.close = function (): void { this.removeAttribute('open'); };
});

async function openDialog(): Promise<void> {
  render(DeleteAccount);
  await fireEvent.click(screen.getByRole('button', { name: 'Delete your account' }));
}

test('Should_LeaveAccountUntouched_WhenConfirmationIsCancelled', async (): Promise<void> => {
  // Arrange
  await openDialog();
  await fireEvent.input(screen.getByLabelText('Current password'), { target: { value: 'secret' } });
  // Act
  await fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
  // Assert
  expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  expect(mutate).not.toHaveBeenCalled();
  await fireEvent.click(screen.getByRole('button', { name: 'Delete your account' }));
  expect(screen.getByLabelText('Current password')).toHaveValue('');
});

test('Should_ShowPendingAndSignOut_WhenDeletionSucceeds', async (): Promise<void> => {
  // Arrange
  let finish: (() => void) | undefined;
  vi.mocked(mutate).mockImplementation((): Promise<void> => new Promise<void>((resolve): void => { finish = resolve; }));
  await openDialog();
  await fireEvent.input(screen.getByLabelText('Current password'), { target: { value: 'secret' } });
  // Act
  await fireEvent.click(screen.getByRole('button', { name: 'Permanently delete account' }));
  // Assert
  expect(screen.getByRole('button', { name: 'Deleting account…' })).toBeDisabled();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
  expect(mutate).toHaveBeenCalledWith('/api/users/me', 'DELETE', { password: 'secret' });
  finish?.();
  await waitFor((): void => { expect(signedOut).toHaveBeenCalledOnce(); });
  expect(get(sessionMessage)).toContain('have been deleted');
});

test('Should_KeepDialogOpenAndClearPassword_WhenDeletionFails', async (): Promise<void> => {
  // Arrange
  vi.mocked(mutate).mockRejectedValue(new Error('Incorrect password'));
  await openDialog();
  await fireEvent.input(screen.getByLabelText('Current password'), { target: { value: 'wrong' } });
  // Act
  await fireEvent.click(screen.getByRole('button', { name: 'Permanently delete account' }));
  // Assert
  expect(await screen.findByRole('alert')).toHaveTextContent('Incorrect password');
  expect(screen.getByRole('dialog')).toBeVisible();
  expect(screen.getByLabelText('Current password')).toHaveValue('');
  expect(signedOut).not.toHaveBeenCalled();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
});
