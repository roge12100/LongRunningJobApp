import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobResultComponent } from './job-result';

describe('JobResult', () => {
  let component: JobResultComponent;
  let fixture: ComponentFixture<JobResultComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobResultComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobResultComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
