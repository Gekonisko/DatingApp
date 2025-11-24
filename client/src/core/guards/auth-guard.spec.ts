import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection, runInInjectionContext } from '@angular/core';
import { AccountService } from '../services/account-service';
import { ToastService } from '../services/toast-service';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth-guard';

describe('authGuard', () => {
  let accountServiceMock: any;
  let toastMock: any;

  // pomocnicze "puste" route/state
  const dummyRoute = {} as ActivatedRouteSnapshot;
  const dummyState = {} as RouterStateSnapshot;

  beforeEach(() => {
    accountServiceMock = {
      currentUser: jasmine.createSpy('currentUser')
    };

    toastMock = {
      error: jasmine.createSpy('error')
    };

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: AccountService, useValue: accountServiceMock },
        { provide: ToastService, useValue: toastMock }
      ]
    });
  });

  it('should allow access when currentUser exists', () => {
    accountServiceMock.currentUser.and.returnValue({ id: 1 });

    const result = TestBed.runInInjectionContext(() =>
      authGuard(dummyRoute, dummyState)
    );

    expect(result).toBeTrue();
    expect(toastMock.error).not.toHaveBeenCalled();
  });

  it('should block access and show toast when currentUser is null', () => {
    accountServiceMock.currentUser.and.returnValue(null);

    const result = TestBed.runInInjectionContext(() =>
      authGuard(dummyRoute, dummyState)
    );

    expect(result).toBeFalse();
    expect(toastMock.error).toHaveBeenCalledWith('You shall not pass');
  });
});
