import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideZonelessChangeDetection } from '@angular/core';
import { of, delay, firstValueFrom } from 'rxjs';

import { loadingInterceptor } from './loading-interceptor';
import { BusyService } from '../services/busy-service';

describe('loadingInterceptor (Zoneless)', () => {
  let http: HttpClient;
  let busyService: jasmine.SpyObj<BusyService>;

  beforeEach(() => {
    busyService = jasmine.createSpyObj('BusyService', ['busy', 'idle']);

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: BusyService, useValue: busyService },
        provideHttpClient(withInterceptors([loadingInterceptor])),
      ],
    });

    http = TestBed.inject(HttpClient);
  });

  function mockResponse(data: any): HttpResponse<any> {
    return new HttpResponse({ status: 200, body: data });
  }

  it('should call busy() before request and idle() after finalize', async () => {
    const response = new HttpResponse({ status: 200, body: { value: 1 } });

    const nextSpy = jasmine
      .createSpy()
      .and.returnValue(of(response).pipe(delay(50))); // reduced delay for test speed

    const req = { method: 'GET', url: '/test' } as any;

    await TestBed.runInInjectionContext(async () => {
      await firstValueFrom(loadingInterceptor(req, nextSpy));
    });

    expect(busyService.busy).toHaveBeenCalledTimes(1);
    expect(busyService.idle).toHaveBeenCalledTimes(1);
  });

  it('should delay the response by 50ms', async () => {
    const response = mockResponse({ delayed: true });

    spyOn(http, 'get').and.returnValue(of(response).pipe(delay(50)));

    let received = false;

    const promise = TestBed.runInInjectionContext(() => {
      return firstValueFrom(http.get('/delay-test'));
    }).then(() => (received = true));

    expect(received).toBeFalse();

    await promise;
    expect(received).toBeTrue();
  });

  it('should not call next() if GET response is cached', () => {
    const response = new HttpResponse({ status: 200, body: { first: true } });

    const nextSpy = jasmine.createSpy().and.returnValue(of(response));

    const req = { method: 'GET', url: '/cached' } as any;

    TestBed.runInInjectionContext(() => {
      // first invocation should call through
      loadingInterceptor(req, nextSpy).subscribe();
    });

    expect(nextSpy).toHaveBeenCalledTimes(1);

    const nextSpy2 = jasmine.createSpy().and.returnValue(of(response));

    TestBed.runInInjectionContext(() => {
      let cached: any;
      loadingInterceptor(req, nextSpy2).subscribe(res => (cached = res));
      expect(cached).toEqual(response);
    });

    expect(nextSpy2).not.toHaveBeenCalled();
  });
});
