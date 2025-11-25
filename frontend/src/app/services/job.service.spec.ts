import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { JobService } from './job.service';
import { 
  CreateJobRequest, 
  CreateJobResponse, 
  CancelJobResponse, 
  JobStatus 
} from '../models/job.model';

describe('JobService', () => {
  let service: JobService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        JobService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    
    service = TestBed.inject(JobService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should create a job', () => {
    const mockRequest: CreateJobRequest = {
      input: 'test input'
    };

    const mockResponse: CreateJobResponse = {
      jobId: '123e4567-e89b-12d3-a456-426614174000',
      status: JobStatus.Queued,
      createdAt: new Date().toISOString(),
      hubUrl: 'http://localhost:8080/hub/job-progress'
    };

    service.createJob(mockRequest).subscribe({
      next: (response) => {
        expect(response).toEqual(mockResponse);
        expect(response.jobId).toBeTruthy();
        expect(response.status).toBe(JobStatus.Queued);
      }
    });

    const req = httpMock.expectOne('http://localhost:8080/api/jobs');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRequest);
    req.flush(mockResponse);
  });

  it('should cancel a job', () => {
    const jobId = '123e4567-e89b-12d3-a456-426614174000';
    const mockResponse: CancelJobResponse = {
      success: true,
      message: 'Job cancelled successfully'
    };

    service.cancelJob(jobId).subscribe({
      next: (response) => {
        expect(response).toEqual(mockResponse);
        expect(response.success).toBe(true);
      }
    });

    const req = httpMock.expectOne(`http://localhost:8080/api/jobs/${jobId}/cancel`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });
});