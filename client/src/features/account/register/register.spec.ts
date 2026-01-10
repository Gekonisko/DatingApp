import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { Register } from './register';
import { AccountService } from '../../../core/services/account-service';
import { By } from '@angular/platform-browser';

describe('Register Component (Zoneless)', () => {
  let fixture: ComponentFixture<Register>;
  let component: Register;
  let accountServiceMock: jasmine.SpyObj<AccountService>;

  beforeEach(async () => {
    accountServiceMock = jasmine.createSpyObj('AccountService', ['register']);

    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideZonelessChangeDetection(),
        { provide: AccountService, useValue: accountServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should emit cancelRegister when cancel() is called', () => {
    const spy = jasmine.createSpy('cancelSpy');
    component.cancelRegister.subscribe(spy);

    component.cancel();

    expect(spy).toHaveBeenCalledOnceWith(false);
  });

  it('should emit cancelRegister when Cancel button is clicked', () => {
    const spy = jasmine.createSpy('cancelSpy');
    component.cancelRegister.subscribe(spy);

    const cancelBtn = fixture.debugElement.query(By.css('button[type="button"]'));
    cancelBtn.triggerEventHandler('click', {});

    expect(spy).toHaveBeenCalledOnceWith(false);
  });

  it('should submit form and call register()', () => {
    const spy = spyOn(component, 'register');
    
    // make forms valid first
    const creds = (component as any).credentialsForm;
    const profile = (component as any).profileForm;
    creds.controls['email'].setValue('a@b.com');
    creds.controls['displayName'].setValue('User');
    creds.controls['password'].setValue('abcd');
    creds.controls['confirmPassword'].setValue('abcd');

    profile.controls['dateOfBirth'].setValue('1990-01-01');
    profile.controls['city'].setValue('City');
    profile.controls['country'].setValue('Country');
    profile.controls['gender'].setValue('male');

    // Move to step 2 where Register button is shown
    (component as any).currentStep.set(2);
    
    fixture.detectChanges();

    // Find Register button by its text content (it uses type="button" not submit)
    const buttons = fixture.debugElement.queryAll(By.css('button'));
    const registerButton = buttons.find(btn => btn.nativeElement.textContent.trim() === 'Register');
    expect(registerButton).toBeTruthy('Register button should exist');
    
    registerButton!.nativeElement.click();
    fixture.detectChanges();

    expect(spy).toHaveBeenCalled();
  });
});
