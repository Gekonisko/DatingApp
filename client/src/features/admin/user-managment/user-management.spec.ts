import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { UserManagement } from './user-management';
import { AdminService } from '../../../core/services/admin-service';

describe('UserManagment', () => {
  let component: UserManagement;
  let fixture: ComponentFixture<UserManagement>;
  const mockAdminService: any = {
    getUserWithRoles: jasmine.createSpy('getUserWithRoles'),
    updateUserRoles: jasmine.createSpy('updateUserRoles')
  };

  beforeEach(async () => {
    mockAdminService.getUserWithRoles.and.returnValue(of([
      { id: '1', displayName: 'A', email: 'a@x', token: '', roles: ['Member'] },
      { id: '2', displayName: 'B', email: 'b@x', token: '', roles: ['Admin'] }
    ]));
    mockAdminService.updateUserRoles.and.returnValue(of(['Admin', 'Member']));

    await TestBed.configureTestingModule({
      imports: [UserManagement],
      providers: [{ provide: AdminService, useValue: mockAdminService }]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads users on init via getUserWithRoles', () => {
    expect(mockAdminService.getUserWithRoles).toHaveBeenCalled();
    expect((component as any).users().length).toBe(2);
  });

  it('toggleRole adds and removes roles on selectedUser', () => {
    const user = { id: '10', displayName: 'X', email: 'x@x', token: '', roles: ['Member'] } as any;
    component.openRolesModal = () => {}; // avoid showing dialog
    (component as any).selectedUser = user;

    // add role
    component.toggleRole({ target: { checked: true } } as any, 'Admin');
    expect(user.roles).toContain('Admin');

    // remove role
    component.toggleRole({ target: { checked: false } } as any, 'Member');
    expect(user.roles).not.toContain('Member');
  });

  it('updateRoles calls AdminService.updateUserRoles and updates users and closes modal', () => {
    const user = { id: '1', displayName: 'A', email: 'a@x', token: '', roles: ['Member'] } as any;
    (component as any).users.set([user]);
    (component as any).selectedUser = user;
    const closeSpy = jasmine.createSpy('close');
    (component as any).rolesModal = { nativeElement: { close: closeSpy } } as any;

    component.updateRoles();

    expect(mockAdminService.updateUserRoles).toHaveBeenCalledWith('1', ['Member']);
    expect((component as any).users()[0].roles).toEqual(['Admin', 'Member']);
    expect(closeSpy).toHaveBeenCalled();
  });
});
