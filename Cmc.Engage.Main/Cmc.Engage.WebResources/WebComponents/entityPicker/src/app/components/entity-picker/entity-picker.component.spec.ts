import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EntityPickerComponent } from './entity-picker.component';

describe('EntityPickerComponent', () => {
  let component: EntityPickerComponent;
  let fixture: ComponentFixture<EntityPickerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EntityPickerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EntityPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
