import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FacultySurveyGridComponent } from './faculty-survey-grid.component';

describe('FacultySurveyGridComponent', () => {
  let component: FacultySurveyGridComponent;
  let fixture: ComponentFixture<FacultySurveyGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FacultySurveyGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FacultySurveyGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
