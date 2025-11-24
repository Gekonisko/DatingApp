import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Register } from './register';
import { provideZonelessChangeDetection } from '@angular/core';
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

  it('should submit form and call register()', async () => {
    const spy = spyOn(component, 'register');
    
    const form = fixture.debugElement.query(By.css('form'));
    form.triggerEventHandler('ngSubmit', {});
    fixture.detectChanges();

    await fixture.whenStable();

    expect(spy).toHaveBeenCalled();
  });
});
