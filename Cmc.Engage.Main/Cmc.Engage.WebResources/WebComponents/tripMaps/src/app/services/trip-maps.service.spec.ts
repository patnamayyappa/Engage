import { TestBed, inject } from '@angular/core/testing';

import { TripMapsService } from './trip-maps.service';

describe('TripMapsService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TripMapsService]
    });
  });

  it('should be created', inject([TripMapsService], (service: TripMapsService) => {
    expect(service).toBeTruthy();
  }));
});
