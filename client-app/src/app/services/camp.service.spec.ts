import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CampService, Speaker } from './camp.service';

describe('CampService - getSpeakers', () => {
  let service: CampService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CampService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CampService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call GET /api/speakers and return the response', () => {
    const mockSpeakers: Speaker[] = [
      { speakerId: 1, firstName: 'Alice', lastName: 'Johnson', company: 'TechCo' },
      { speakerId: 2, firstName: 'Bob', lastName: 'Lee' },
    ];

    let result: Speaker[] = [];
    service.getSpeakers().subscribe((speakers) => {
      result = speakers;
    });

    const req = httpMock.expectOne('/api/speakers');
    expect(req.request.method).toBe('GET');
    req.flush(mockSpeakers);

    expect(result).toEqual(mockSpeakers);
  });

  it('should return an empty array when no speakers exist', () => {
    let result: Speaker[] = [{ speakerId: 99, firstName: 'Placeholder', lastName: 'Data' }];
    service.getSpeakers().subscribe((speakers) => {
      result = speakers;
    });

    const req = httpMock.expectOne('/api/speakers');
    req.flush([]);

    expect(result).toEqual([]);
  });
});
