import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { TextInput } from './text-input';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, TextInput],
  template: `
    <form [formGroup]="form">
      <app-text-input formControlName="name"></app-text-input>
    </form>
  `,
})
class HostComponent {
  form = new FormGroup({ name: new FormControl('') });
}

describe('TextInput', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, TextInput, HostComponent],
      providers: [provideZonelessChangeDetection()]
    }).compileComponents();
  });

  it('should create and register as valueAccessor', () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();

    const debugEl = fixture.debugElement.query(By.directive(TextInput));
    const component = debugEl.componentInstance as TextInput;

    expect(component).toBeTruthy();
    // The host form control should be available on the component
    expect(component.control).toBeTruthy();
    // The component should have registered itself as valueAccessor on the NgControl
    expect((component as any).ngControl.valueAccessor).toBe(component);
  });
});
