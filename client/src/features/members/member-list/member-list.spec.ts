import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of } from 'rxjs';

import { MemberList } from './member-list';
import { MemberService } from '../../../core/services/member-service';
import { Member } from '../../../types/member';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { provideLocationMocks } from '@angular/common/testing';

describe('MemberList (Zoneless)', () => {
  let fixture: ComponentFixture<MemberList>;
  let component: MemberList;

  const mockMembers: Member[] = [
    {
      id: '1',
      displayName: 'John Doe',
      dateOfBirth: new Date('1990-01-01').toString(),
      city: 'New York',
      country: 'USA',
      imageUrl: '/john.jpg',
      created: '',
      lastActive: '',
      gender: '',
    },
    {
      id: '2',
      displayName: 'Jane Smith',
      dateOfBirth: new Date('1992-01-01').toString(),
      city: 'Chicago',
      country: 'USA',
      imageUrl: '/jane.jpg',
      created: '',
      lastActive: '',
      gender: '',
    }
  ];

  

  const memberServiceMock = {
    getMembers: jasmine.createSpy('getMembers').and.returnValue(of(mockMembers))
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberList],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideLocationMocks(),
        { provide: MemberService, useValue: memberServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberList);
    component = fixture.componentInstance;
    fixture.detectChanges();

    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should load members from service', () => {
    expect(memberServiceMock.getMembers).toHaveBeenCalled();
  });

  it('should render a MemberCard for each member', () => {
    const html: HTMLElement = fixture.nativeElement;
    const cards = html.querySelectorAll('app-member-card');

    expect(cards.length).toBe(2);
  });

  it('should pass correct input "member" to each MemberCard', () => {
    const cardElements = fixture.debugElement.queryAll(
      d => d.name === 'app-member-card'
    );

    expect(cardElements.length).toBe(2);

    const firstCard = cardElements[0];
    const secondCard = cardElements[1];

    expect(firstCard.componentInstance.member()).toEqual(mockMembers[0]);
    expect(secondCard.componentInstance.member()).toEqual(mockMembers[1]);
  });
});
