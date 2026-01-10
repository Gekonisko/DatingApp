import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { ConfirmDialog } from './confirm-dialog';

describe('ConfirmDialog', () => {
  let component: ConfirmDialog;
  let fixture: ComponentFixture<ConfirmDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmDialog],
      providers: [provideZonelessChangeDetection()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfirmDialog);
    component = fixture.componentInstance;
    // provide a fake dialogRef to avoid DOM dialog methods
    component.dialogRef = { nativeElement: { showModal: () => {}, close: () => {} } } as any;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('open shows modal and returns a promise resolved by confirm', async () => {
    const showSpy = spyOn(component.dialogRef.nativeElement, 'showModal');
    const p = component.open('Delete?');
    expect(showSpy).toHaveBeenCalled();
    const resPromise = p.then(v => v);
    // simulate user confirming
    component.confirm();
    await expectAsync(resPromise).toBeResolvedTo(true);
  });

  it('cancel resolves promise to false and closes dialog', async () => {
    const closeSpy = spyOn(component.dialogRef.nativeElement, 'close');
    const p = component.open('Cancel test');
    component.cancel();
    await expectAsync(p).toBeResolvedTo(false);
    expect(closeSpy).toHaveBeenCalled();
  });
});
