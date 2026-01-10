import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { By } from '@angular/platform-browser';

import { DeleteButton } from './delete-button';

describe('DeleteButton', () => {
  let component: DeleteButton;
  let fixture: ComponentFixture<DeleteButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteButton],
      providers: [provideZonelessChangeDetection()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteButton);
    // ensure inputs are set via componentRef for standalone signal-based inputs
    fixture.componentRef.setInput('disabled', false);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

   it('should emit clickEvent when button is clicked', () => {
    spyOn(component.clickEvent, 'emit');

    const button: HTMLButtonElement = fixture.debugElement.query(By.css('button')).nativeElement;
    button.click();

    expect(component.clickEvent.emit).toHaveBeenCalledTimes(1);
    expect(component.clickEvent.emit).toHaveBeenCalledWith(jasmine.any(Event));
  });

  it('should NOT emit clickEvent when disabled is true', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();

    spyOn(component.clickEvent, 'emit');

    const button: HTMLButtonElement = fixture.debugElement.query(By.css('button')).nativeElement;
    button.click();

    expect(component.clickEvent.emit).not.toHaveBeenCalled();
  });
});
