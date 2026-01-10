import { TestBed, ComponentFixture } from '@angular/core/testing';
import { MemberCard } from './member-card';
import { provideZonelessChangeDetection } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';
import { Member } from '../../../types/member';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { RouterLink } from '@angular/router';
import { By } from '@angular/platform-browser';

describe('MemberCard (Zoneless)', () => {
  let fixture: ComponentFixture<MemberCard>;
  let component: MemberCard;

  const mockMember: Member = {
      id: "1",
      displayName: 'John Doe',
      dateOfBirth: new Date('1995-01-01').toString(),
      city: 'New York',
      imageUrl: '/test.jpg',
      created: '',
      lastActive: '',
      gender: '',
      country: ''
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MemberCard,
        RouterTestingModule
      ],
      providers: [
        provideZonelessChangeDetection()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MemberCard);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('member', mockMember);
    fixture.detectChanges();
  });

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should render member name, city and image', () => {
    const html = fixture.nativeElement as HTMLElement;

    expect(html.textContent).toContain('John Doe');
    expect(html.textContent).toContain('New York');

    const img = fixture.debugElement.query(By.css('img'))!.nativeElement as HTMLImageElement;
    expect(img.src).toContain('/test.jpg');
  });

  it('should calculate age using AgePipe', () => {
    const html = fixture.nativeElement as HTMLElement;

    const pipe = new AgePipe();
    const expectedAge = pipe.transform(mockMember.dateOfBirth);

    const nameAgeText = html.textContent?.trim() || '';

    expect(nameAgeText).toContain(`${expectedAge}`);
  });
});
