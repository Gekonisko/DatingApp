import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { MemberProfile } from './member-profile';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../../types/member';
import { AccountService } from '../../../core/services/account-service';
import { provideHttpClient } from '@angular/common/http';

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

  const accountServiceMock = {
    getMember: jasmine.createSpy()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberProfile],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberProfile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });


  it('should create component', () => {
    expect(component).toBeTruthy();
  });
});