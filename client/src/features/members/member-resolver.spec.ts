import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ResolveFn, Router, RouterStateSnapshot } from '@angular/router';
import { of, EMPTY } from 'rxjs';

import { memberResolver } from './member-resolver';
import { MemberService } from '../../core/services/member-service';

describe('memberResolver', () => {
  const executeResolver: ResolveFn<any> = (...resolverParameters) =>
    TestBed.runInInjectionContext(() => memberResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()]
    });
  });

  it('returns member when id param is present', (done) => {
    const mockMember = { id: '42', username: 'test' } as any;
    const mockMemberService = { getMember: jasmine.createSpy('getMember').and.returnValue(of(mockMember)) } as Partial<MemberService> as MemberService;
    const mockRouter = { navigateByUrl: jasmine.createSpy('navigateByUrl') } as any as Router;

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: MemberService, useValue: mockMemberService },
        { provide: Router, useValue: mockRouter }
      ]
    });

    const route = { paramMap: { get: (key: string) => '42' } } as any;

    const result = executeResolver(route, {} as RouterStateSnapshot);
    expect(mockMemberService.getMember).toHaveBeenCalledWith('42');

    result.subscribe({
      next: (m: any) => {
        expect(m).toEqual(mockMember);
        expect(mockRouter.navigateByUrl).not.toHaveBeenCalled();
        done();
      },
      error: done.fail
    });
  });

  it('navigates to not-found and returns EMPTY when id is missing', (done) => {
    const mockMemberService = { getMember: jasmine.createSpy('getMember') } as Partial<MemberService> as MemberService;
    const mockRouter = { navigateByUrl: jasmine.createSpy('navigateByUrl') } as any as Router;

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: MemberService, useValue: mockMemberService },
        { provide: Router, useValue: mockRouter }
      ]
    });

    const route = { paramMap: { get: (key: string) => null } } as any;

    const result = executeResolver(route, {} as RouterStateSnapshot);

    // EMPTY completes without emitting values
    let emitted = false;
    result.subscribe({
      next: () => emitted = true,
      complete: () => {
        expect(emitted).toBeFalse();
        expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/not-found');
        done();
      },
      error: done.fail
    });
  });
});
