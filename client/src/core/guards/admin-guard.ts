import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account-service';
import { ToastService } from '../services/toast-service';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toast = inject(ToastService);
  const user = accountService.currentUser();

  if (user?.roles?.includes('Admin') || user?.roles?.includes('Moderator')) {
    return true;
  }

  toast.error('Access denied');
  return false;
};
// Pure helper for unit tests and reuse without Angular DI
export function canActivateAdmin(user: { roles?: string[] } | null, toast: { error: (msg: string) => void } | null): boolean {
  if (user?.roles?.includes('Admin') || user?.roles?.includes('Moderator')) return true;
  toast?.error('Access denied');
  return false;
}