import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { LikesService } from './likes-service';
import { environment } from '../../environments/environment';

describe('LikesService', () => {
  let service: LikesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(LikesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getLikeIds sets the likeIds signal from server', () => {
    service.getLikeIds();
    const req = httpMock.expectOne(environment.apiUrl + 'likes/list');
    expect(req.request.method).toBe('GET');
    req.flush(['a', 'b']);
    expect(service.likeIds()).toEqual(['a', 'b']);
  });

  it('clearLikeIds clears the signal', () => {
    service.likeIds.set(['x']);
    expect(service.likeIds()).toEqual(['x']);
    service.clearLikeIds();
    expect(service.likeIds()).toEqual([]);
  });

  it('toggleLike posts and adds/removes id from signal', () => {
    service.likeIds.set([]);

    service.toggleLike('t1');
    const req1 = httpMock.expectOne(environment.apiUrl + 'likes/t1');
    expect(req1.request.method).toBe('POST');
    req1.flush({});
    expect(service.likeIds()).toContain('t1');

    // toggle again -> should remove
    service.toggleLike('t1');
    const req2 = httpMock.expectOne(environment.apiUrl + 'likes/t1');
    expect(req2.request.method).toBe('POST');
    req2.flush({});
    expect(service.likeIds()).not.toContain('t1');
  });

  it('getLikes issues GET with correct params and returns result', () => {
    let resp: any;
    service.getLikes('liked', 2, 10).subscribe(r => (resp = r));

    const req = httpMock.expectOne(r => r.url === environment.apiUrl + 'likes'
      && r.params.get('predicate') === 'liked'
      && r.params.get('pageNumber') === '2'
      && r.params.get('pageSize') === '10');
    expect(req.request.method).toBe('GET');

    const mockResp = { result: [], pagination: { currentPage: 2, pageSize: 10, totalCount: 0, totalPages: 0 } };
    req.flush(mockResp);
    expect(resp).toEqual(mockResp);
  });
});
