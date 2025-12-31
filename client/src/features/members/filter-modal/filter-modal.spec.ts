import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterModal } from './filter-modal';

describe('FilterModal', () => {
  let component: FilterModal;
  let fixture: ComponentFixture<FilterModal>;

  beforeEach(async () => {
    // ensure localStorage does not interfere with defaults
    localStorage.removeItem('filters');

    await TestBed.configureTestingModule({
      imports: [FilterModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterModal);
    component = fixture.componentInstance;
    // provide a fake modalRef to avoid DOM dialog operations
    component.modalRef = { nativeElement: { showModal: () => {}, close: () => {} } } as any;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('has default memberParams', () => {
    const mp = (component as any).memberParams();
    expect(mp.minAge).toBeGreaterThanOrEqual(18);
    expect(mp.maxAge).toBeGreaterThanOrEqual(mp.minAge);
  });

  it('onMinAgeChange enforces minimum 18', () => {
    (component as any).memberParams.set({ ...((component as any).memberParams()), minAge: 10 });
    component.onMinAgeChange();
    expect((component as any).memberParams().minAge).toBe(18);
  });

  it('onMaxAgeChange enforces max >= min', () => {
    (component as any).memberParams.set({ ...((component as any).memberParams()), minAge: 30, maxAge: 25 });
    component.onMaxAgeChange();
    expect((component as any).memberParams().maxAge).toBe((component as any).memberParams().minAge);
  });

  it('submit emits submitData and closes modal', () => {
    const submitSpy = jasmine.createSpy('submit');
    const closeSpy = jasmine.createSpy('close');
    (component as any).submitData = { emit: submitSpy };
    (component as any).closeModal = { emit: closeSpy };
    const closeNative = jasmine.createSpy('nativeClose');
    component.modalRef = { nativeElement: { close: closeNative } } as any;

    (component as any).memberParams.set({ ...((component as any).memberParams()), minAge: 20 });
    component.submit();

    expect(submitSpy).toHaveBeenCalledWith((component as any).memberParams());
    expect(closeNative).toHaveBeenCalled();
    expect(closeSpy).toHaveBeenCalled();
  });

  it('open calls native showModal', () => {
    const showSpy = jasmine.createSpy('showModal');
    component.modalRef = { nativeElement: { showModal: showSpy } } as any;
    component.open();
    expect(showSpy).toHaveBeenCalled();
  });
});
