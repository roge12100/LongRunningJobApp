import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobStatusHeaderComponent } from './job-status-header';

describe('JobStatusHeader', () => {
  let component: JobStatusHeaderComponent;
  let fixture: ComponentFixture<JobStatusHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobStatusHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobStatusHeaderComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
