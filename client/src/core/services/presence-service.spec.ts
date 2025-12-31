import { TestBed } from '@angular/core/testing';
import { HubConnectionState } from '@microsoft/signalr';

import { PresenceService } from './presence-service';

describe('PresenceService', () => {
  let service: PresenceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PresenceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('stopHubConnection calls stop when connected', async () => {
    const stopSpy = jasmine.createSpy('stop').and.returnValue(Promise.resolve());
    (service as any).hubConnection = { state: HubConnectionState.Connected, stop: stopSpy } as any;
    service.stopHubConnection();
    expect(stopSpy).toHaveBeenCalled();
  });

  it('stopHubConnection does not call stop when not connected', () => {
    const stopSpy = jasmine.createSpy('stop');
    (service as any).hubConnection = { state: HubConnectionState.Disconnected, stop: stopSpy } as any;
    service.stopHubConnection();
    expect(stopSpy).not.toHaveBeenCalled();
  });

  it('onlineUsers signal can be set, updated and cleared', () => {
    expect(service.onlineUsers()).toEqual([]);
    service.onlineUsers.set(['a']);
    expect(service.onlineUsers()).toEqual(['a']);
    service.onlineUsers.update(list => [...list, 'b']);
    expect(service.onlineUsers()).toEqual(['a', 'b']);
    service.onlineUsers.set([]);
    expect(service.onlineUsers()).toEqual([]);
  });
});
