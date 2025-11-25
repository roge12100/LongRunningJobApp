import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobProgressComponent } from './job-progress';

describe('JobProgress', () => {
  let component: JobProgressComponent;
  let fixture: ComponentFixture<JobProgressComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobProgressComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobProgressComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
