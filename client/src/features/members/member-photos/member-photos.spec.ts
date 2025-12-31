import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { of } from 'rxjs';

import { MemberPhotos } from './member-photos';
import { MemberService } from '../../../core/services/member-service';
import { AccountService } from '../../../core/services/account-service';
import { ActivatedRoute } from '@angular/router';
import { Photo } from '../../../types/member';

describe('MemberPhotos (Zoneless)', () => {
  let fixture: ComponentFixture<MemberPhotos>;
  let component: MemberPhotos;

  const mockPhotos: Photo[] = [
    {
        id: 1, url: '/photo-1.jpg',
        memberId: '',
        isApproved: true
    },
    {
        id: 2, url: '/photo-2.jpg',
        memberId: '',
        isApproved: true
    }
  ];

  const memberServiceMock = {
    getMemberPhotos: jasmine.createSpy('getMemberPhotos').and.returnValue(of(mockPhotos))
  };

  const accountServiceMock = {
    currentUser: () => ({ id: '123', imageUrl: null }),
    setCurrentUser: jasmine.createSpy('setCurrentUser')
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
        { provide: AccountService, useValue: accountServiceMock },
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

  it('should upload a photo and add to photos list, set editMode false and set main if no imageUrl', () => {
    const newPhoto = { id: 3, url: '/photo-3.jpg', memberId: '', isApproved: true } as Photo;

    // extend memberServiceMock for upload
    (memberServiceMock as any).uploadPhoto = jasmine.createSpy('uploadPhoto').and.returnValue(of(newPhoto));
    (memberServiceMock as any).editMode = { set: jasmine.createSpy('set') };
    const memberSignal: any = (() => ({ id: '123', imageUrl: null }));
    memberSignal.update = jasmine.createSpy('update');
    (memberServiceMock as any).member = memberSignal;

    // re-create component with extended mock
    TestBed.overrideProvider(MemberService as any, { useValue: memberServiceMock });
    fixture = TestBed.createComponent(MemberPhotos);
    component = fixture.componentInstance;

    // call upload
    const file = new File(['x'], 'photo.jpg', { type: 'image/jpeg' });
    component.onUploadImage(file);

    // verify behaviors
    expect((memberServiceMock as any).uploadPhoto).toHaveBeenCalled();
    expect((memberServiceMock as any).editMode.set).toHaveBeenCalledWith(false);
    expect(memberSignal.update).toHaveBeenCalled();
    const photosArray = (component as any).photos();
    expect(photosArray.find((p: Photo) => p.id === 3)).toBeTruthy();
  });

  it('should set main photo by calling service and updating locals', () => {
    const photo = { id: 1, url: '/photo-1.jpg' } as Photo;
    (memberServiceMock as any).setMainPhoto = jasmine.createSpy('setMainPhoto').and.returnValue(of({}));
    const memberSignal2: any = (() => ({ id: '123', imageUrl: '/old.jpg' }));
    memberSignal2.update = jasmine.createSpy('update');
    (memberServiceMock as any).member = memberSignal2;

    TestBed.overrideProvider(MemberService as any, { useValue: memberServiceMock });
    TestBed.overrideProvider(AccountService as any, { useValue: accountServiceMock });

    fixture = TestBed.createComponent(MemberPhotos);
    component = fixture.componentInstance;

    component.setMainPhoto(photo);

    expect((memberServiceMock as any).setMainPhoto).toHaveBeenCalledWith(photo);
    expect((memberServiceMock as any).member.update).toHaveBeenCalled();
    expect(accountServiceMock.setCurrentUser).toHaveBeenCalled();
  });

  it('should delete photo and remove from photos signal', () => {
    (memberServiceMock as any).deletePhoto = jasmine.createSpy('deletePhoto').and.returnValue(of({}));

    TestBed.overrideProvider(MemberService as any, { useValue: memberServiceMock });
    fixture = TestBed.createComponent(MemberPhotos);
    component = fixture.componentInstance;

    // initialize photos
    (component as any).photos.set([...mockPhotos]);

    component.deletePhoto(1);

    expect((memberServiceMock as any).deletePhoto).toHaveBeenCalledWith(1);
    const remaining = (component as any).photos();
    expect(remaining.find((p: Photo) => p.id === 1)).toBeUndefined();
  });
});
