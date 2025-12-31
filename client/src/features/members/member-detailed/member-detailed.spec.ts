import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MemberDetailed } from './member-detailed';
import { provideZonelessChangeDetection } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { BehaviorSubject, Subject, of } from 'rxjs';
import { Member } from '../../../types/member';
import { MemberService } from '../../../core/services/member-service';
import { AccountService } from '../../../core/services/account-service';
import { PresenceService } from '../../../core/services/presence-service';
import { LikesService } from '../../../core/services/likes-service';
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
    // provide paramMap as an observable-like with subscribe
    paramMap: {
      subscribe: (fn: any) => fn({ get: (key: string) => '1' })
    } as any,
    snapshot: { paramMap: new Map([['id', '1']]) } as any,
    firstChild: { snapshot: { title: 'Profile' } } as any
  };

  const mockRouter: Partial<Router> = {
    events: routerEvents$
  };

  const memberSignal: any = (() => mockMember);
  memberSignal.update = jasmine.createSpy('update');
  memberSignal.set = jasmine.createSpy('set');

  const memberServiceMock: any = {
    member: memberSignal,
    editMode: (() => false) as any
  };

  const accountServiceMock = {
    currentUser: () => ({ id: '1' }),
    setCurrentUser: jasmine.createSpy('setCurrentUser')
  };

  const presenceServiceMock = {
    onlineUsers: () => []
  };

  const likesServiceMock = {
    likeIds: () => [],
    toggleLike: jasmine.createSpy('toggleLike')
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
        { provide: AccountService, useValue: accountServiceMock },
        { provide: PresenceService as any, useValue: presenceServiceMock },
        { provide: LikesService as any, useValue: likesServiceMock }
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

    expect((component as any).title()).toBe('Photos');
  });

  it('should show "Member not found" when member is undefined', () => {
    // make the member signal return undefined
    (memberServiceMock as any).member = (() => undefined);

    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Member not found');
  });
});
