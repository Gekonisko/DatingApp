import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MemberDetailed } from './member-detailed';
import { provideZonelessChangeDetection } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd, RouterModule, RouterLink, provideRouter } from '@angular/router';
import { BehaviorSubject, Observable, of, Subject } from 'rxjs';
import { Member } from '../../../types/member';
import { provideLocationMocks } from '@angular/common/testing';

describe('MemberDetailed (Zoneless)', () => {
  let fixture: ComponentFixture<MemberDetailed>;
  let component: MemberDetailed;

  const mockMember: Member = {
      id: "1",
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

  const mockActivatedRoute: Partial<ActivatedRoute> = {
    data: data$,
    firstChild: {
      snapshot: {
        title: 'Profile'
      }
    } as any
  };

  const routerEvents$ = new Subject<any>();

  const mockRouter: Partial<Router> = {
    events: routerEvents$
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberDetailed],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideLocationMocks(),
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
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
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;

    expect(html.textContent).toContain('John Doe');
    expect(html.textContent).toContain('New York');
    expect(html.querySelector('img')!.src).toContain('/test.jpg');
  });

  it('should update title after NavigationEnd event', async () => {
    (mockActivatedRoute.firstChild as any).snapshot.title = 'Photos';

    routerEvents$.next(new NavigationEnd(1, '/members/1/photos', '/members/1/photos'));

    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.title()).toBe('Profile');
  });

  it('should show "Member not found" when member is undefined', () => {
    (fixture.componentInstance as any).member.set(undefined);

    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Member not found');
  });
});
