import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { PhotoManagement } from './photo-management';
import { AdminService } from '../../../core/services/admin-service';

describe('PhotoManagment', () => {
  let component: PhotoManagement;
  let fixture: ComponentFixture<PhotoManagement>;
  const mockAdminService: any = {
    getPhotosForApproval: jasmine.createSpy('getPhotosForApproval'),
    approvePhoto: jasmine.createSpy('approvePhoto'),
    rejectPhoto: jasmine.createSpy('rejectPhoto')
  };

  beforeEach(async () => {
    mockAdminService.getPhotosForApproval.and.returnValue(of([
      { id: 1, url: 'u1', memberId: 'm1', isApproved: false },
      { id: 2, url: 'u2', memberId: 'm2', isApproved: false }
    ]));
    mockAdminService.approvePhoto.and.returnValue(of({}));
    mockAdminService.rejectPhoto.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [PhotoManagement],
      providers: [{ provide: AdminService, useValue: mockAdminService }]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads photos on init via getPhotosForApproval', () => {
    expect(mockAdminService.getPhotosForApproval).toHaveBeenCalled();
    expect(component.photos().length).toBe(2);
  });

  it('approvePhoto removes approved photo from photos', () => {
    component.photos.set([{ id: 10, url: 'u10', memberId: 'm', isApproved: false }, { id: 20, url: 'u20', memberId: 'm', isApproved: false }]);
    mockAdminService.approvePhoto.calls.reset();
    mockAdminService.approvePhoto.and.returnValue(of({}));

    component.approvePhoto(10);
    expect(mockAdminService.approvePhoto).toHaveBeenCalledWith(10);
    expect(component.photos().find(p => (p as any).id === 10)).toBeUndefined();
  });

  it('rejectPhoto removes rejected photo from photos', () => {
    component.photos.set([{ id: 30, url: 'u30', memberId: 'm', isApproved: false }, { id: 40, url: 'u40', memberId: 'm', isApproved: false }]);
    mockAdminService.rejectPhoto.calls.reset();
    mockAdminService.rejectPhoto.and.returnValue(of({}));

    component.rejectPhoto(30);
    expect(mockAdminService.rejectPhoto).toHaveBeenCalledWith(30);
    expect(component.photos().find(p => (p as any).id === 30)).toBeUndefined();
  });
});
