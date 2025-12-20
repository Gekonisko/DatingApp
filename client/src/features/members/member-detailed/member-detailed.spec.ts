import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MemberDetailed } from './member-detailed';
import { provideZonelessChangeDetection } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { BehaviorSubject, Subject, of } from 'rxjs';
import { Member } from '../../../types/member';
import { MemberService } from '../../../core/services/member-service';
import { AccountService } from '../../../core/services/account-service';
import { provideLocationMocks } from '@angular/common/testing';

describe('MemberDetailed (Zoneless)', () => {
  let fixture: ComponentFixture<MemberDetailed>;
  let component: MemberDetailed;

  const mockMember: Member = {
    id: '1',
    displayName: 'John Doe',
    dateOfBirth: new Date('1990-01-01').toString(),
    city: 'New York',
    country: 'USA',
    imageUrl: '/test.jpg',
    created: '',
    lastActive: '',
    gender: ''
  };

  const data$ = new BehaviorSubject({ member: mockMember });
  const routerEvents$ = new Subject<any>();

  const mockActivatedRoute: Partial<ActivatedRoute> = {
    data: data$,
    snapshot: { paramMap: new Map([['id', '1']]) } as any,
    firstChild: { snapshot: { title: 'Profile' } } as any
  };

  const mockRouter: Partial<Router> = {
    events: routerEvents$
  };

  const memberServiceMock = {
    member: jasmine.createSpy().and.returnValue(of(mockMember))
  };

  const accountServiceMock = {
    currentUser: () => ({ id: '1' })
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberDetailed],
      providers: [
        provideZonelessChangeDetection(),
        provideLocationMocks(),
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: MemberService, useValue: memberServiceMock },
        { provide: AccountService, useValue: accountServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberDetailed);
    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should set member from route data', () => {
    data$.next({ member: mockMember });

    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;

    expect(html.textContent).toContain('John Doe');
    expect(html.textContent).toContain('New York');

    const img = html.querySelector('img');
    expect(img).toBeTruthy();
    expect(img!.src).toContain('/test.jpg');
  });

  it('should update title after NavigationEnd event', () => {
    (mockActivatedRoute.firstChild as any).snapshot.title = 'Photos';
    routerEvents$.next(new NavigationEnd(1, '/members/1/photos', '/members/1/photos'));

    fixture.detectChanges();

    expect(component.title()).toBe('Photos');
  });

  it('should show "Member not found" when member is undefined', () => {
    component['member'].set(undefined);

    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Member not found');
  });
});
