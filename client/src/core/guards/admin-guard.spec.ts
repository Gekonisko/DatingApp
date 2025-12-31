import { canActivateAdmin } from './admin-guard';

describe('canActivateAdmin', () => {
  it('allows a user with Admin role', () => {
    const mockUser = { roles: ['Admin'] } as any;
    const mockToast = { error: jasmine.createSpy('error') } as any;

    const result = canActivateAdmin(mockUser, mockToast);
    expect(result).toBeTrue();
    expect(mockToast.error).not.toHaveBeenCalled();
  });

  it('allows a user with Moderator role', () => {
    const mockUser = { roles: ['Moderator'] } as any;
    const mockToast = { error: jasmine.createSpy('error') } as any;

    const result = canActivateAdmin(mockUser, mockToast);
    expect(result).toBeTrue();
    expect(mockToast.error).not.toHaveBeenCalled();
  });

  it('denies when user has no admin/moderator role', () => {
    const mockUser = { roles: ['User'] } as any;
    const mockToast = { error: jasmine.createSpy('error') } as any;

    const result = canActivateAdmin(mockUser, mockToast);
    expect(result).toBeFalse();
    expect(mockToast.error).toHaveBeenCalledWith('Access denied');
  });

  it('denies when not authenticated', () => {
    const mockToast = { error: jasmine.createSpy('error') } as any;

    const result = canActivateAdmin(null, mockToast);
    expect(result).toBeFalse();
    expect(mockToast.error).toHaveBeenCalledWith('Access denied');
  });
});
