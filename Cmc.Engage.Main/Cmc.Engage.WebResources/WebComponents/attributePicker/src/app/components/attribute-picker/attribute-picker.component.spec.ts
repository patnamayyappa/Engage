import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AttributePickerComponent } from './attribute-picker.component';

describe('AttributePickerComponent', () => {
  let component: AttributePickerComponent;
  let fixture: ComponentFixture<AttributePickerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AttributePickerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AttributePickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
