import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  CreateJobRequest, 
  CreateJobResponse,
  CancelJobResponse 
} from '../models/job.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Create and start a new job
   * POST /api/jobs
   */
  createJob(request: CreateJobRequest): Observable<CreateJobResponse> {
    return this.http.post<CreateJobResponse>(this.apiUrl, request);
  }

  /**
   * Cancel a running job
   * POST /api/jobs/{jobId}/cancel
   */
  cancelJob(jobId: string): Observable<CancelJobResponse> {
    return this.http.post<CancelJobResponse>(
      `${this.apiUrl}/${jobId}/cancel`, 
      {}
    );
  }
}