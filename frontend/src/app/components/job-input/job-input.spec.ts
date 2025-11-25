import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobInputComponent } from './job-input';

describe('JobInput', () => {
  let component: JobInputComponent;
  let fixture: ComponentFixture<JobInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobInputComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobInputComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
