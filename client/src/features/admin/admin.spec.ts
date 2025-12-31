import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NgZone, NO_ERRORS_SCHEMA } from '@angular/core';
import { AccountService } from '../../core/services/account-service';

import { Admin } from './admin';

describe('Admin', () => {
  let component: Admin;
  let fixture: ComponentFixture<Admin>;
  let ngZoneMock: Partial<NgZone>;

  beforeEach(async () => {
    const observableLike = () => ({ subscribe: (fn: any) => ({ unsubscribe: () => {} }) });
    ngZoneMock = {
      run: (fn: any) => fn(),
      runOutsideAngular: (fn: any) => fn(),
      onMicrotaskEmpty: observableLike() as any,
      onStable: observableLike() as any,
      onUnstable: observableLike() as any,
      onError: observableLike() as any
    };

    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [{ provide: NgZone, useValue: ngZoneMock }],
      schemas: [NO_ERRORS_SCHEMA]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Admin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('has default activeTab = photos and shows photo tab', () => {
    expect(component.activeTab).toBe('photos');

    const html = fixture.nativeElement as HTMLElement;
    const tabs = Array.from(html.querySelectorAll('button')) as HTMLButtonElement[];
    const photoTab = tabs.find(b => b.textContent?.trim().includes('Photo moderation'));
    expect(photoTab).toBeTruthy();
    expect(photoTab?.classList.contains('tab-active')).toBeTrue();
  });

  it('hides roles tab for non-admin and shows for admin', async () => {
    // re-create with non-admin
    fixture.destroy();

    const mockAccount = { currentUser: () => ({ id: '1', roles: ['User'] }) } as any;
    TestBed.resetTestingModule();

    const ngZoneMock2 = ngZoneMock as any;
    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [{ provide: NgZone, useValue: ngZoneMock2 }, { provide: AccountService, useValue: mockAccount }],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    const fixture2 = TestBed.createComponent(Admin);
    fixture2.detectChanges();
    const html2 = fixture2.nativeElement as HTMLElement;
    const rolesButton = Array.from(html2.querySelectorAll('button')).find(b => b.textContent?.includes('User management'));
    // hidden via [hidden] attribute when not admin
    expect(rolesButton?.hasAttribute('hidden')).toBeTrue();

    // now admin
    fixture2.destroy();
    const mockAccountAdmin = { currentUser: () => ({ id: '1', roles: ['Admin'] }) } as any;
    await TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [{ provide: NgZone, useValue: ngZoneMock2 }, { provide: AccountService, useValue: mockAccountAdmin }],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    const fixture3 = TestBed.createComponent(Admin);
    fixture3.detectChanges();
    const html3 = fixture3.nativeElement as HTMLElement;
    const rolesButtonVisible = Array.from(html3.querySelectorAll('button')).find(b => b.textContent?.includes('User management'));
    expect(rolesButtonVisible).toBeTruthy();
  });
});
