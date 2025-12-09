import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClient, HttpEvent, HttpResponse } from '@angular/common/http';
import { provideHttpClient } from '@angular/common/http';
import { withInterceptors } from '@angular/common/http';
import { of } from 'rxjs';

import { loadingInterceptor } from './loading-interceptor';
import { BusyService } from '../services/busy-service';

describe('loadingInterceptor (InterceptorFn)', () => {
  let http: HttpClient;
  let busyService: jasmine.SpyObj<BusyService>;

  beforeEach(() => {
    busyService = jasmine.createSpyObj('BusyService', ['busy', 'idle']);

    TestBed.configureTestingModule({
      providers: [
        { provide: BusyService, useValue: busyService },
        provideHttpClient(withInterceptors([loadingInterceptor])),
      ],
    });

    http = TestBed.inject(HttpClient);
  });

  function mockResponse(data: any): HttpResponse<any> {
    return new HttpResponse({ status: 200, body: data });
  }

  it('should call busy() before request and idle() after finalize', fakeAsync(() => {
    const response = mockResponse({ value: 1 });

    spyOn(http, 'get').and.returnValue(of(response));

    http.get('/test').subscribe();

    expect(busyService.busy).toHaveBeenCalledTimes(1);

    tick(500);

    expect(busyService.idle).toHaveBeenCalledTimes(1);
  }));


  it('should delay the response by 500ms', fakeAsync(() => {
    const response = mockResponse({ delayed: true });

    spyOn(http, 'get').and.returnValue(of(response));

    let received = false;
    http.get('/delay-test').subscribe(() => (received = true));

    expect(received).toBeFalse();

    tick(499);
    expect(received).toBeFalse();

    tick(1); 
    expect(received).toBeTrue();
  }));

  it('should not call next() if GET response is cached', fakeAsync(() => {
    const response = mockResponse({ first: true });

    const nextSpy = jasmine.createSpy().and.returnValue(of(response));

    const req = { method: 'GET', url: '/cached' } as any;

    loadingInterceptor(req, nextSpy).subscribe();
    tick(500);

    expect(nextSpy).toHaveBeenCalledTimes(1);

    const nextSpy2 = jasmine.createSpy();
    loadingInterceptor(req, nextSpy2).subscribe(res => {
      expect(res).toEqual(response);
    });

    expect(nextSpy2).not.toHaveBeenCalled();
  }));
});
