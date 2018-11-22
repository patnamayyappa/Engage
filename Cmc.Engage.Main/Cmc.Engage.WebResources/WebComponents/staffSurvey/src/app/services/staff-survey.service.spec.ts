import { TestBed, inject } from '@angular/core/testing';

import { StaffSurveyService } from './staff-survey.service';

describe('StaffSurveyService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StaffSurveyService]
    });
  });

  it('should be created', inject([StaffSurveyService], (service: StaffSurveyService) => {
    expect(service).toBeTruthy();
  }));
});
