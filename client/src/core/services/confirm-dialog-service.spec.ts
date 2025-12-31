import { TestBed } from '@angular/core/testing';

import { ConfirmDialogService } from './confirm-dialog-service';

describe('ConfirmDialogService', () => {
  let service: ConfirmDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConfirmDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('throws when confirming without a registered component', () => {
    expect(() => service.confirm('x' as any)).toThrowError('Confirm dialog component is not registered');
  });

  it('calls open on the registered component and returns the resolved value', async () => {
    let receivedMessage: string | undefined;
    const mockComponent = {
      open: (msg: string) => {
        receivedMessage = msg;
        return Promise.resolve(true);
      }
    } as any;

    service.register(mockComponent);
    const result = await service.confirm('Delete item?');
    expect(receivedMessage).toBe('Delete item?');
    expect(result).toBeTrue();
  });

  it('uses default message when none provided', async () => {
    let receivedMessage: string | undefined;
    const mockComponent = {
      open: (msg: string) => {
        receivedMessage = msg;
        return Promise.resolve(false);
      }
    } as any;

    service.register(mockComponent);
    const result = await service.confirm();
    expect(receivedMessage).toBe('Are you sure?');
    expect(result).toBeFalse();
  });
});
