import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ContactSurveyComponent } from './contact-survey.component';

describe('ContactSurveyComponent', () => {
  let component: ContactSurveyComponent;
  let fixture: ComponentFixture<ContactSurveyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ContactSurveyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ContactSurveyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
