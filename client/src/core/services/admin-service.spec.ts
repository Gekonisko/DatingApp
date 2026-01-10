import { TestBed } from '@angular/core/testing';
import { NgZone, provideZonelessChangeDetection } from '@angular/core';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AdminService } from './admin-service';
import { environment } from '../../environments/environment';

describe('AdminService', () => {
  let service: AdminService;
  let httpMock: HttpTestingController;
  const base = environment.apiUrl;

  beforeEach(() => {
    const observableLike = () => ({ subscribe: (fn: any) => ({ unsubscribe: () => {} }) });
    const ngZoneMock: Partial<NgZone> = {
      run: (fn: any) => fn(),
      runOutsideAngular: (fn: any) => fn(),
      onMicrotaskEmpty: observableLike() as any,
      onStable: observableLike() as any,
      onUnstable: observableLike() as any,
      onError: observableLike() as any
    };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [provideZonelessChangeDetection(), AdminService, { provide: NgZone, useValue: ngZoneMock }]
    });

    service = TestBed.inject(AdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch users with roles', () => {
    const mockUsers = [{ id: '1', username: 'a', roles: ['Admin'] } as any];

    service.getUserWithRoles().subscribe(users => {
      expect(users).toEqual(mockUsers);
    });

    const req = httpMock.expectOne(base + 'admin/users-with-roles');
    expect(req.request.method).toBe('GET');
    req.flush(mockUsers);
  });

  it('should update user roles', () => {
    const userId = '123';
    const roles = ['Admin','Moderator'];
    const returned = roles;

    service.updateUserRoles(userId, roles).subscribe(r => {
      expect(r).toEqual(returned);
    });

    const req = httpMock.expectOne(base + 'admin/edit-roles/' + userId + '?roles=' + roles);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush(returned);
  });

  it('should get photos for approval', () => {
    const photos = [{ id: 1, url: 'u' } as any];

    service.getPhotosForApproval().subscribe(p => {
      expect(p).toEqual(photos);
    });

    const req = httpMock.expectOne(base + 'admin/photos-to-moderate');
    expect(req.request.method).toBe('GET');
    req.flush(photos);
  });

  it('should approve a photo', () => {
    const id = 5;

    service.approvePhoto(id).subscribe(res => {
      expect(res).toBeTruthy();
    });

    const req = httpMock.expectOne(base + 'admin/approve-photo/' + id);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush({});
  });

  it('should reject a photo', () => {
    const id = 6;

    service.rejectPhoto(id).subscribe(res => {
      expect(res).toBeTruthy();
    });

    const req = httpMock.expectOne(base + 'admin/reject-photo/' + id);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush({});
  });
});