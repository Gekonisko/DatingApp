import { getTestBed,  } from '@angular/core/testing';
import { BrowserTestingModule, platformBrowserTesting } from '@angular/platform-browser/testing';
import { NgZone } from '@angular/core';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';

// Initialize the Angular testing environment in zoneless mode (noop NgZone).
const platform: any = (platformBrowserTesting as unknown as any)({ ngZone: 'noop' });

getTestBed().initTestEnvironment(
  BrowserTestingModule,
  platform
);

// Minimal noop NgZone mock for zoneless tests (satisfies Angular injection needs).
const ngZoneMock: Partial<NgZone> = {
  run: (fn: any) => fn(),
  runOutsideAngular: (fn: any) => fn(),
  onStable: { subscribe: (_: any) => ({ unsubscribe: () => {} }) } as any,
  onUnstable: { subscribe: (_: any) => ({ unsubscribe: () => {} }) } as any,
  onMicrotaskEmpty: { subscribe: (_: any) => ({ unsubscribe: () => {} }) } as any,
  onError: { subscribe: (_: any) => ({ unsubscribe: () => {} }) } as any
};

// Globally add common testing modules/providers so individual specs don't need to repeat them.
getTestBed().configureTestingModule({
  imports: [HttpClientTestingModule],
  providers: [
    { provide: NgZone, useValue: ngZoneMock },
    {
      provide: Router,
      useValue: {
        createUrlTree: (..._args: any[]) => ({}),
        navigate: () => Promise.resolve(true),
        events: { subscribe: () => {} },
        url: '/',
        serializeUrl: (_: any) => '/',
      } as any,
    },
  ],
});
