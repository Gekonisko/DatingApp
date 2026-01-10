import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { MessageService } from './message-service';
import { environment } from '../../environments/environment';

describe('MessageService', () => {
  let service: MessageService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        provideZonelessChangeDetection(),
        { provide: 'AccountService', useValue: {} },
        { provide: 'ToastService', useValue: {} }
      ] as any
    });
    service = TestBed.inject(MessageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getMessages issues GET with params and returns result', () => {
    let resp: any;
    service.getMessages('Inbox', 1, 5).subscribe(r => resp = r);

    const req = httpMock.expectOne(r => r.url === environment.apiUrl + 'messages' && r.params.get('container') === 'Inbox');
    expect(req.request.method).toBe('GET');

    const mock = { result: [], pagination: { currentPage: 1, pageSize: 5, totalCount: 0, totalPages: 0 } };
    req.flush(mock);
    expect(resp).toEqual(mock);
  });

  it('getMessageThread GETs thread for member', () => {
    let resp: any;
    service.getMessageThread('123').subscribe(r => resp = r);
    const req = httpMock.expectOne(environment.apiUrl + 'messages/thread/123');
    expect(req.request.method).toBe('GET');
    const mock = [{ id: 'm1', content: 'hi' }];
    req.flush(mock);
    expect(resp).toEqual(mock);
  });

  it('deleteMessage issues DELETE', () => {
    service.deleteMessage('m1').subscribe();
    const req = httpMock.expectOne(environment.apiUrl + 'messages/m1');
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('sendMessage invokes hubConnection.SendMessage when hubConnection is present', async () => {
    const invokeSpy = jasmine.createSpy('invoke').and.returnValue(Promise.resolve('ok'));
    (service as any).hubConnection = { invoke: invokeSpy } as any;
    const res = service.sendMessage('r1', 'hello');
    expect(invokeSpy).toHaveBeenCalledWith('SendMessage', { recipientId: 'r1', content: 'hello' });
    await expectAsync(res as Promise<any>).toBeResolvedTo('ok');
  });
});
