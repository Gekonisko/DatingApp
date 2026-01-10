import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { Paginator } from './paginator';

describe('Paginator', () => {
  let component: Paginator;
  let fixture: ComponentFixture<Paginator>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Paginator],
      providers: [provideZonelessChangeDetection()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Paginator);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('computes lastItemIndex and emits pageChange on page number change', () => {
    // set total count via componentRef inputs (standalone signal inputs)
    fixture.componentRef.setInput('totalCount', 95);
    fixture.componentRef.setInput('totalPages', 10);
    fixture.detectChanges();

    // initial lastItemIndex = 10
    expect(component.lastItemIndex()).toBe(10);

    const emitSpy = jasmine.createSpy('emit');
    (component as any).pageChange = { emit: emitSpy } as any;

    component.onPageChange(3);
    expect((component as any).pageNumber()).toBe(3);
    expect(emitSpy).toHaveBeenCalledWith({ pageNumber: 3, pageSize: 10 });
  });

  it('changes pageSize when select element provided and emits', () => {
    const emitSpy = jasmine.createSpy('emit');
    (component as any).pageChange = { emit: emitSpy } as any;

    const fakeSelect = { value: '20' } as any;
    component.onPageChange(undefined, fakeSelect as any);

    expect((component as any).pageSize()).toBe(20);
    expect(emitSpy).toHaveBeenCalledWith({ pageNumber: 1, pageSize: 20 });
  });
});
