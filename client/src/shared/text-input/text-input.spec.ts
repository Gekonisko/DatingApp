import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { NgControl } from '@angular/forms';

import { TextInput } from './text-input';

describe('TextInput', () => {
  let component: TextInput;
  let fixture: ComponentFixture<TextInput>;
  const mockControl = new FormControl('');
  const mockNgControl: Partial<NgControl> = { control: mockControl, valueAccessor: null };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TextInput],
      providers: [{ provide: NgControl, useValue: mockNgControl }]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TextInput);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('assigns itself as valueAccessor on the injected NgControl', () => {
    expect((mockNgControl as any).valueAccessor).toBe(component);
  });

  it('control getter returns the injected FormControl', () => {
    expect(component.control).toBe(mockControl);
  });
});
