import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteButton } from './delete-button';

describe('DeleteButton', () => {
  let component: DeleteButton;
  let fixture: ComponentFixture<DeleteButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteButton]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteButton);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

   it('should emit clickEvent when button is clicked', () => {
    spyOn(component.clickEvent, 'emit');

    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    button.click();

    expect(component.clickEvent.emit).toHaveBeenCalledTimes(1);
    expect(component.clickEvent.emit).toHaveBeenCalledWith(jasmine.any(Event));
  });

  it('should NOT emit clickEvent when disabled is true', () => {
    component.disabled.apply(true);
    fixture.detectChanges();

    spyOn(component.clickEvent, 'emit');

    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    button.click();

    expect(component.clickEvent.emit).not.toHaveBeenCalled();
  });
});
