import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';

import { jwtInterceptor } from './jwt-interceptor';
import { AccountService } from '../services/account-service';
import { provideZonelessChangeDetection } from '@angular/core';

describe('jwtInterceptor (Zoneless)', () => {
  let httpMock: HttpTestingController;

  let accountServiceMock: any;

  beforeEach(() => {
    accountServiceMock = {
      currentUser: jasmine.createSpy()
    };

    TestBed.configureTestingModule({
      imports: [],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withInterceptors([jwtInterceptor])),
        provideHttpClientTesting(),
        { provide: AccountService, useValue: accountServiceMock }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add Authorization header when user exists', () => {
    accountServiceMock.currentUser.and.returnValue({
      token: 'abc123'
    });

    const http = TestBed.inject(HttpClient) as any;

    http.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');

    expect(req.request.headers.get('Authorization')).toBe('Bearer abc123');

    req.flush({});
  });

  it('should NOT add Authorization header when user is null', () => {
    accountServiceMock.currentUser.and.returnValue(null);

    const http = TestBed.inject(HttpClient) as any;

    http.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');

    expect(req.request.headers.has('Authorization')).toBeFalse();

    req.flush({});
  });
});
