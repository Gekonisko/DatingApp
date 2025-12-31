import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of } from 'rxjs';

import { InitService } from './init-service';
import { AccountService } from './account-service';
import { User } from '../../types/user';

describe('InitService', () => {
  let service: InitService;
  let accountServiceMock: any;

  const mockUser: User = {
    id: '1',
    displayName: 'John Doe',
    token: 'abc123',
    email: '',
    roles: ['User']
  };

  beforeEach(() => {
    accountServiceMock = {
      refreshToken: jasmine.createSpy('refreshToken'),
      setCurrentUser: jasmine.createSpy('setCurrentUser'),
      startTokenRefreshInterval: jasmine.createSpy('startTokenRefreshInterval')
    };

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        InitService,
        { provide: AccountService, useValue: accountServiceMock }
      ]
    });

    service = TestBed.inject(InitService);
  });

  it('calls setCurrentUser and startTokenRefreshInterval when refreshToken returns a user', (done) => {
    accountServiceMock.refreshToken.and.returnValue(of(mockUser));

    service.init().subscribe(result => {
      expect(result).toEqual(mockUser);
      expect(accountServiceMock.setCurrentUser).toHaveBeenCalledWith(mockUser);
      expect(accountServiceMock.startTokenRefreshInterval).toHaveBeenCalled();
      done();
    });
  });

  it('does not call setCurrentUser when refreshToken returns null', (done) => {
    accountServiceMock.refreshToken.and.returnValue(of(null));

    service.init().subscribe(result => {
      expect(result).toBeNull();
      expect(accountServiceMock.setCurrentUser).not.toHaveBeenCalled();
      expect(accountServiceMock.startTokenRefreshInterval).not.toHaveBeenCalled();
      done();
    });
  });
});
