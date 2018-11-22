import { TestBed, inject } from '@angular/core/testing';

import { InternalConnectionsService } from './internal-connections.service';

describe('InternalConnectionsService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [InternalConnectionsService]
    });
  });

  it('should be created', inject([InternalConnectionsService], (service: InternalConnectionsService) => {
    expect(service).toBeTruthy();
  }));
});
