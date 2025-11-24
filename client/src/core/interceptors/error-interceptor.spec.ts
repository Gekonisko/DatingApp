import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { throwError } from 'rxjs';

import { errorInterceptor } from './error-interceptor';
import { ToastService } from '../services/toast-service';

describe('errorInterceptor (Zoneless)', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let toastMock: any;
  let router: Router;
  let navigateSpy: jasmine.Spy;

  beforeEach(() => {

    toastMock = {
      error: jasmine.createSpy('error')
    };

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),

        { provide: ToastService, useValue: toastMock },
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    navigateSpy = spyOn(router, 'navigateByUrl');
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should show toast on 400 without modelStateErrors', () => {
    http.get('/test').subscribe({
      error: () => {}
    });

    const req = httpMock.expectOne('/test');
    req.flush('Bad request', { status: 400, statusText: 'Bad Request' });

    expect(toastMock.error).toHaveBeenCalledWith('Bad request');
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('should throw modelStateErrors array on 400 with modelStateErrors', () => {
    http.get('/test').subscribe({
      error: (err) => {
        expect(err).toEqual(['Error1', 'Error2']);
      }
    });

    const req = httpMock.expectOne('/test');
    req.flush(
      {
        errors: {
          email: ['Error1'],
          password: ['Error2']
        }
      },
      { status: 400, statusText: 'Bad Request' }
    );

    expect(toastMock.error).not.toHaveBeenCalled();
  });

  it('should show toast on 401', () => {
    http.get('/test').subscribe({
      error: () => {}
    });

    const req = httpMock.expectOne('/test');
    req.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(toastMock.error).toHaveBeenCalledWith('Unauthorized');
  });

  it('should navigate to /not-found on 404', () => {
    http.get('/test').subscribe({
      error: () => {}
    });

    const req = httpMock.expectOne('/test');
    req.flush({}, { status: 404, statusText: 'Not Found' });

    expect(navigateSpy).toHaveBeenCalledWith('/not-found');
  });

  it('should navigate to /server-error with state on 500', () => {
    const serverErr = { message: 'server exploded' };

    http.get('/test').subscribe({
      error: () => {}
    });

    const req = httpMock.expectOne('/test');
    req.flush(serverErr, { status: 500, statusText: 'Server Error' });

    expect(navigateSpy).toHaveBeenCalledWith('/server-error', {
      state: { error: serverErr }
    });
  });

  it('should show default error on unknown status code', () => {
    http.get('/test').subscribe({
      error: () => {}
    });

    const req = httpMock.expectOne('/test');
    req.flush({}, { status: 418, statusText: "I'm a teapot" });

    expect(toastMock.error).toHaveBeenCalledWith('Something went wrong');
  });
});
