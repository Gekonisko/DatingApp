import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { BusyService } from './busy-service';

describe('BusyService', () => {
  let service: BusyService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), BusyService]
    });
    service = TestBed.inject(BusyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
