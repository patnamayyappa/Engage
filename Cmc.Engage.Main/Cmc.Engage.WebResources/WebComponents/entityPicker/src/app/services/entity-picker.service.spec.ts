import { TestBed } from '@angular/core/testing';

import { EntityPickerService } from './entity-picker.service';

describe('EntityPickerService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: EntityPickerService = TestBed.get(EntityPickerService);
    expect(service).toBeTruthy();
  });
});
