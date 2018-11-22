import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TripMapsComponent } from './trip-maps.component';

describe('TripMapsComponent', () => {
  let component: TripMapsComponent;
  let fixture: ComponentFixture<TripMapsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TripMapsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TripMapsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
