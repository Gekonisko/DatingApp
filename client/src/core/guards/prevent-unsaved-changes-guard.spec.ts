import { preventUnsavedChangesGuard } from './prevent-unsaved-changes-guard';
import { FormGroup } from '@angular/forms';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('preventUnsavedChangesGuard', () => {
  let component: any;

  // Dummy arguments required by CanDeactivateFn
  const route = {} as ActivatedRouteSnapshot;
  const currentState = {} as RouterStateSnapshot;
  const nextState = {} as RouterStateSnapshot;

  beforeEach(() => {
    component = {
      editForm: new FormGroup({})
    };
  });

  const callGuard = () =>
    preventUnsavedChangesGuard(component, route, currentState, nextState);

  it('should return true when form is not dirty', () => {
    component.editForm.markAsPristine();

    const result = callGuard();

    expect(result).toBeTrue();
  });

  it('should call confirm when form is dirty', () => {
    component.editForm.markAsDirty();

    const confirmSpy = spyOn(window, 'confirm').and.returnValue(true);

    const result = callGuard();

    expect(confirmSpy).toHaveBeenCalledWith(
      'Are you sure you want to continue? All unsaved changes will be lost'
    );
    expect(result).toBeTrue();
  });

  it('should return false if confirm is cancelled', () => {
    component.editForm.markAsDirty();

    spyOn(window, 'confirm').and.returnValue(false);

    const result = callGuard();

    expect(result).toBeFalse();
  });

  it('should return true if editForm is missing', () => {
    component.editForm = undefined;

    const result = callGuard();

    expect(result).toBeTrue();
  });
});
