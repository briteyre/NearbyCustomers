import { TestBed } from '@angular/core/testing';
import { SpeakersComponent } from './speakers.component';
import { CampService, Speaker } from '../../services/camp.service';
import { provideAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

describe('SpeakersComponent', () => {
  let mockCampService: jasmine.SpyObj<CampService>;

  const mockSpeakers: Speaker[] = [
    {
      speakerId: 1,
      firstName: 'Jane',
      lastName: 'Smith',
      company: 'Acme Corp',
      twitter: 'janesmith',
      gitHub: 'janesmith',
      blogUrl: 'https://janesmith.dev',
    },
    {
      speakerId: 2,
      firstName: 'John',
      middleName: 'A',
      lastName: 'Doe',
    },
  ];

  beforeEach(async () => {
    mockCampService = jasmine.createSpyObj<CampService>('CampService', ['getSpeakers']);
    mockCampService.getSpeakers.and.returnValue(of(mockSpeakers));

    await TestBed.configureTestingModule({
      imports: [SpeakersComponent],
      providers: [
        { provide: CampService, useValue: mockCampService },
        provideAnimations(),
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(SpeakersComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should call getSpeakers on init', () => {
    const fixture = TestBed.createComponent(SpeakersComponent);
    fixture.detectChanges();
    expect(mockCampService.getSpeakers).toHaveBeenCalledOnceWith();
  });

  it('should populate speakers from the service', () => {
    const fixture = TestBed.createComponent(SpeakersComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.speakers).toEqual(mockSpeakers);
  });

  it('should set loading to false after data is loaded', () => {
    const fixture = TestBed.createComponent(SpeakersComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.loading).toBeFalse();
  });

  it('should set loading to false on error', () => {
    mockCampService.getSpeakers.and.returnValue(throwError(() => new Error('Server error')));
    const fixture = TestBed.createComponent(SpeakersComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.loading).toBeFalse();
  });

  it('should display speaker names in the template', () => {
    const fixture = TestBed.createComponent(SpeakersComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Jane');
    expect(compiled.textContent).toContain('Smith');
    expect(compiled.textContent).toContain('John');
    expect(compiled.textContent).toContain('Doe');
  });

  it('should show no-speakers message when list is empty', () => {
    mockCampService.getSpeakers.and.returnValue(of([]));
    const fixture = TestBed.createComponent(SpeakersComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('No speakers found.');
  });
});
