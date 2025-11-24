import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { InitService } from './init-service';
import { AccountService } from './account-service';
import { User } from '../../types/user';

describe('InitService (Zoneless)', () => {
  let service: InitService;
  let accountService: AccountService;
  let getItemSpy: jasmine.Spy;

  const mockUser: User = {
      id: '1',
      displayName: 'John Doe',
      token: 'abc123',
      email: ''
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        InitService,
        AccountService
      ]
    });

    service = TestBed.inject(InitService);
    accountService = TestBed.inject(AccountService);
    getItemSpy = spyOn(localStorage, 'getItem');
    spyOn(localStorage, 'getItem').and.callFake((key: string) => null);
    spyOn(localStorage, 'setItem');
  });


  it('should return of(null) when no user in localStorage', (done) => {
    getItemSpy.and.returnValue(null);

    service.init().subscribe(result => {
      expect(result).toBeNull();
      expect(accountService.currentUser()).toBeNull();
      done();
    });
  });


  it('should set currentUser when user exists in localStorage', (done) => {
    getItemSpy.and.returnValue(JSON.stringify(mockUser));

    service.init().subscribe(result => {
      expect(result).toBeNull();
      expect(accountService.currentUser()).toEqual(mockUser);
      done();
    });
  });

});
