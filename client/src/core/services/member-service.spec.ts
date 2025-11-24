import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { InitService } from './init-service';
import { AccountService } from './account-service';
import { User } from '../../types/user';

describe('InitService (Zoneless)', () => {
  let service: InitService;
  let accountService: AccountService;
  let getItemSpy: jasmine.Spy;
  let mockUser: User;

  beforeEach(() => {
    mockUser = {
      id: '1',
      displayName: 'John',
      token: 'abc123',
      email: ''
    };

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withFetch()),
        provideHttpClientTesting(),
        InitService,
        AccountService
      ]
    });

    service = TestBed.inject(InitService);
    accountService = TestBed.inject(AccountService);

    getItemSpy = spyOn(localStorage, 'getItem');
    spyOn(localStorage, 'setItem');
    spyOn(localStorage, 'removeItem');
  });

  it('should return null when no user in localStorage', (done) => {
    getItemSpy.and.returnValue(null);

    service.init().subscribe(value => {
      expect(value).toBeNull();
      expect(accountService.currentUser()).toBeNull();
      done();
    });
  });

  it('should set currentUser when user exists', (done) => {
    getItemSpy.and.returnValue(JSON.stringify(mockUser));

    service.init().subscribe(value => {
      expect(accountService.currentUser()).toEqual(mockUser);
      done();
    });
  });
});
