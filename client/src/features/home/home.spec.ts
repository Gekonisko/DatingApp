import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Home } from './home';
import { Register } from "../account/register/register";
import { By } from '@angular/platform-browser';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';

describe('Home (zoneless)', () => {
  let fixture: ComponentFixture<Home>;
  let component: Home;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withFetch())
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should switch registerMode', () => {
    component.showRegister(true);
    fixture.detectChanges();

    expect(component['registerMode']()).toBeTrue();
  });

  it('should handle async logic without fakeAsync', () => {
    // zoneless doesn't need timeouts - components update synchronously
    component.showRegister(true);
    fixture.detectChanges();

    expect(component['registerMode']()).toBeTrue();
  });
});
