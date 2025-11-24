import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';

import { AccountService } from './account-service';
import { environment } from '../../environments/environment';

describe('AccountService (Zoneless)', () => {
  let service: AccountService;
  let httpMock: HttpTestingController;

  const mockUser = {
    id: '1',
    token: 'abc123',
    email: 'test@test.com',
    displayName: 'John',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        AccountService
      ]
    });

    service = TestBed.inject(AccountService);
    httpMock = TestBed.inject(HttpTestingController);

    spyOn(localStorage, 'setItem');
    spyOn(localStorage, 'removeItem');
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create service', () => {
    expect(service).toBeTruthy();
  });

  it('should send register() request and set current user', () => {
    service.register({ email: 'a', password: 'b', displayName: 'John' })
      .subscribe();

    const req = httpMock.expectOne(environment.apiUrl + 'account/register');

    expect(req.request.method).toBe('POST');

    req.flush(mockUser);

    expect(localStorage.setItem).toHaveBeenCalled();
    expect(service.currentUser()).toEqual(mockUser);
  });

  it('should send login() request and set current user', () => {
    service.login({ email: 'a', password: 'b' }).subscribe();

    const req = httpMock.expectOne(environment.apiUrl + 'account/login');

    expect(req.request.method).toBe('POST');

    req.flush(mockUser);

    expect(service.currentUser()).toEqual(mockUser);
    expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser));
  });

  it('should set current user via setCurrentUser()', () => {
    service.setCurrentUser(mockUser);

    expect(service.currentUser()).toEqual(mockUser);
    expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser));
  });

  it('should logout user', () => {
    service.setCurrentUser(mockUser);

    service.logout();

    expect(service.currentUser()).toBeNull();
    expect(localStorage.removeItem).toHaveBeenCalledWith('user');
  });
});
