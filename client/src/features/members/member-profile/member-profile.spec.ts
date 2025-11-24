import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { MemberProfile } from './member-profile';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../../types/member';

describe('MemberProfile (Zoneless)', () => {
  let fixture: ComponentFixture<MemberProfile>;
  let component: MemberProfile;

  const mockMember: Member = {
    id: '1',
    displayName: 'John Doe',
    dateOfBirth: '',
    city: 'New York',
    country: 'USA',
    imageUrl: '/img.jpg',
    created: '2024-01-10T00:00:00',
    lastActive: '2024-01-12T15:35:00',
    gender: 'male',
    description: 'A test member',
  };

  const data$ = new BehaviorSubject({ member: mockMember });

  const mockActivatedRoute = {
    parent: {
      data: data$
    }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberProfile], 
      providers: [
        provideZonelessChangeDetection(),
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberProfile);
    component = fixture.componentInstance;

    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create component', () => {
    expect(component).toBeTruthy();
  });



  it('should render the member profile data', () => {
    const html = fixture.nativeElement as HTMLElement;

    expect(html.textContent).toContain('About John Doe');
    expect(html.textContent).toContain('A test member');

    expect(html.textContent).toContain('Member since:');

    expect(html.textContent).toContain('Last active:');
  });
});