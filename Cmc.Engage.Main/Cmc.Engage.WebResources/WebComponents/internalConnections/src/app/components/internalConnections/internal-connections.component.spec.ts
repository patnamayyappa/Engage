import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InternalConnectionsComponent } from './internal-connections.component';

describe('InternalConnectionsComponent', () => {
  let component: InternalConnectionsComponent;
  let fixture: ComponentFixture<InternalConnectionsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InternalConnectionsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InternalConnectionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
