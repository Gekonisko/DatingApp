import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of } from 'rxjs';

import { MemberPhotos } from './member-photos';
import { MemberService } from '../../../core/services/member-service';
import { ActivatedRoute } from '@angular/router';
import { Photo } from '../../../types/member';

describe('MemberPhotos (Zoneless)', () => {
  let fixture: ComponentFixture<MemberPhotos>;
  let component: MemberPhotos;

  const mockPhotos: Photo[] = [
    {
        id: 1, url: '/photo-1.jpg',
        memberId: ''
    },
    {
        id: 2, url: '/photo-2.jpg',
        memberId: ''
    }
  ];

  const memberServiceMock = {
    getMemberPhotos: jasmine.createSpy('getMemberPhotos').and.returnValue(of(mockPhotos))
  };

  const mockActivatedRoute = {
    parent: {
      snapshot: {
        paramMap: {
          get: (key: string) => key === 'id' ? '123' : null
        }
      }
    }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberPhotos],   
      providers: [
        provideZonelessChangeDetection(),
        { provide: MemberService, useValue: memberServiceMock },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberPhotos);
    component = fixture.componentInstance;

    fixture.detectChanges();
    await fixture.whenStable();  
    fixture.detectChanges();
  });

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should call getMemberPhotos with the id from route', () => {
    expect(memberServiceMock.getMemberPhotos).toHaveBeenCalledWith('123');
  });

  it('should render photos returned from service', () => {
    const html = fixture.nativeElement as HTMLElement;
    const images = html.querySelectorAll('img');

    const photo1 = Array.from(images).find(img => img.src.includes('photo-1.jpg'));
    const photo2 = Array.from(images).find(img => img.src.includes('photo-2.jpg'));

    expect(photo1).toBeTruthy();
    expect(photo2).toBeTruthy();
  });

  it('should render photoMocks (20 items)', () => {
    const html = fixture.nativeElement as HTMLElement;
    const images = html.querySelectorAll('img');

    expect(images.length).toBe(22);
  });
});
