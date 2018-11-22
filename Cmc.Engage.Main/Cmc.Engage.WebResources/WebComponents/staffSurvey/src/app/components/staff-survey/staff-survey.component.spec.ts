import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffSurveyComponent } from './staff-survey.component';

describe('StaffSurveyComponent', () => {
  let component: StaffSurveyComponent;
  let fixture: ComponentFixture<StaffSurveyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StaffSurveyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StaffSurveyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
