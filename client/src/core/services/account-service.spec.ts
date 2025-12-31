import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';

import { AccountService } from './account-service';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';
import { PresenceService } from './presence-service';

describe('AccountService (Zoneless)', () => {
  let service: AccountService;
  let httpMock: HttpTestingController;

  const mockUser = {
    id: '1',
    token: 'abc123',
    email: 'test@test.com',
    displayName: 'John',
    roles: ['User']
  };

  const likesServiceMock = {
    getLikeIds: jasmine.createSpy('getLikeIds'),
    clearLikeIds: jasmine.createSpy('clearLikeIds')
  };

  const presenceServiceMock = {
    createHubConnection: jasmine.createSpy('createHubConnection'),
    stopHubConnection: jasmine.createSpy('stopHubConnection'),
    hubConnection: undefined
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        AccountService,
        { provide: LikesService as any, useValue: likesServiceMock },
        { provide: PresenceService as any, useValue: presenceServiceMock }
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
    service.register({ email: 'a', password: 'b', displayName: 'John', gender: 'male', dateOfBirth: '1990-01-01', city: 'X', country: 'Y' })
      .subscribe();

    const req = httpMock.expectOne(environment.apiUrl + 'account/register');

    expect(req.request.method).toBe('POST');

    req.flush(mockUser);

    expect(localStorage.setItem).toHaveBeenCalled();
    expect(service.currentUser()).toEqual(mockUser);
  });

  it('should send login() request and set current user and start refresh interval', () => {
    spyOn(service, 'startTokenRefreshInterval');

    service.login({ email: 'a', password: 'b' }).subscribe();

    const req = httpMock.expectOne(environment.apiUrl + 'account/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockUser);

    expect(localStorage.setItem).toHaveBeenCalled();
    expect(service.currentUser()).toEqual(mockUser);
    expect((service as any).startTokenRefreshInterval).toHaveBeenCalled();
  });

  it('should call refreshToken endpoint', () => {
    service.refreshToken().subscribe();
    const req = httpMock.expectOne(environment.apiUrl + 'account/refresh-token');
    expect(req.request.method).toBe('POST');
    req.flush(mockUser);
  });

  it('setCurrentUser should parse roles and start presence/likes', () => {
    const payload = btoa(JSON.stringify({ role: 'Admin' }));
    const token = `a.${payload}.c`;
    const user = { ...mockUser, token } as any;

    service.setCurrentUser(user);

    expect(service.currentUser()?.roles).toContain('Admin');
    expect(likesServiceMock.getLikeIds).toHaveBeenCalled();
    expect(presenceServiceMock.createHubConnection).toHaveBeenCalledWith(user);
  });

  it('logout should call backend and clear local data and stop presence', () => {
    service.logout();

    const req = httpMock.expectOne(environment.apiUrl + 'account/logout');
    expect(req.request.method).toBe('POST');
    req.flush({});

    expect(localStorage.removeItem).toHaveBeenCalled();
    expect(likesServiceMock.clearLikeIds).toHaveBeenCalled();
    expect(service.currentUser()).toBeNull();
    expect(presenceServiceMock.stopHubConnection).toHaveBeenCalled();
  });

  it('should set current user via setCurrentUser()', () => {
    service.setCurrentUser(mockUser);

    expect(service.currentUser()).toEqual(mockUser);
    expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser));
  });

});
