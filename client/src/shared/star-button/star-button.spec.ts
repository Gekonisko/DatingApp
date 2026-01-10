import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';

import { StarButton } from './star-button';

describe('StarButton', () => {
  let component: StarButton;
  let fixture: ComponentFixture<StarButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StarButton],
      providers: [provideZonelessChangeDetection()]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StarButton);
    fixture.componentRef.setInput('isStarred', false);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
