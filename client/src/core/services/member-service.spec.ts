import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MemberService } from './member-service';
import { environment } from '../../environments/environment';
import { Member, Photo } from '../../types/member';

describe('MemberService', () => {
  let service: MemberService;
  let httpMock: HttpTestingController;
  const base = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MemberService]
    });

    service = TestBed.inject(MemberService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should get members with provided params and store filters', () => {
    const mockResult: any = { result: [], pagination: {} };
    const params = { pageNumber: '1', pageSize: '10', minAge: '18', maxAge: '99', orderBy: 'lastActive', gender: 'male' } as any;

    spyOn(localStorage, 'setItem');

    service.getMembers(params).subscribe(res => {
      expect(res).toEqual(mockResult);
    });

    const req = httpMock.expectOne(req => req.url === base + 'members');
    expect(req.request.method).toBe('GET');
    req.flush(mockResult);

    expect(localStorage.setItem).toHaveBeenCalled();
  });

  it('should get a single member and set member signal', () => {
    const mockMember: Member = { id: '1', displayName: 'A', dateOfBirth: '', city: '', country: '', imageUrl: '', created: '', lastActive: '', gender: '' };

    service.getMember('1').subscribe(m => {
      expect(m).toEqual(mockMember);
    });

    const req = httpMock.expectOne(base + 'members/1');
    expect(req.request.method).toBe('GET');
    req.flush(mockMember);
  });

  it('should get member photos', () => {
    const photos: Photo[] = [{ id: 1, url: '/1.jpg', memberId: '1', isApproved: true } as any];

    service.getMemberPhotos('1').subscribe(r => expect(r).toEqual(photos));

    const req = httpMock.expectOne(base + 'members/1/photos');
    expect(req.request.method).toBe('GET');
    req.flush(photos);
  });

  it('should upload photo using FormData', () => {
    const file = new File(['a'], 'a.png', { type: 'image/png' });
    const photo: Photo = { id: 5, url: '/5.png', memberId: '1', isApproved: true } as any;

    service.uploadPhoto(file).subscribe(p => expect(p).toEqual(photo));

    const req = httpMock.expectOne(base + 'members/add-photo');
    expect(req.request.method).toBe('POST');
    // body should be FormData containing 'file'
    expect(req.request.body instanceof FormData).toBeTrue();
    const body = req.request.body as FormData;
    expect(body.has('file')).toBeTrue();
    req.flush(photo);
  });

  it('should set main photo with PUT', () => {
    const photo = { id: 2 } as Photo;

    service.setMainPhoto(photo).subscribe(res => expect(res).toBeTruthy());

    const req = httpMock.expectOne(base + 'members/set-main-photo/' + photo.id);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should delete photo with DELETE', () => {
    service.deletePhoto(7).subscribe(res => expect(res).toBeTruthy());

    const req = httpMock.expectOne(base + 'members/delete-photo/7');
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
