import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { JobStatus } from '../../models/job.model';
import { JobStatusHeaderComponent } from '../job-status-header/job-status-header';

@Component({
  selector: 'app-job-result',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    MessageModule,
    JobStatusHeaderComponent
  ],
  template: `
    <p-card styleClass="mb-3 fade-in">
      <!-- Success Result -->
      @if (status === JobStatus.Completed) {
        <ng-template pTemplate="header">
          <app-job-status-header 
            [status]="status"
            iconColor="var(--p-green-500)"
            textColor="var(--p-green-500)">
          </app-job-status-header>
        </ng-template>

        <div class="result-section">
          <label class="block mb-2 font-semibold">Final Result</label>
          <div class="result-text-display">
            {{ result }}
          </div>

          <div class="flex justify-content-between align-items-center mt-3">
            <div class="text-sm text-color-secondary">
              <div>Total Time: {{ elapsedTime }}</div>
              <div>Characters Processed: {{ result.length }}</div>
            </div>
            
            <p-button
              label="Start New Job"
              icon="pi pi-refresh"
              (onClick)="startNewJob.emit()">
            </p-button>
          </div>
        </div>
      }

      <!-- Cancelled Result -->
      @if (status === JobStatus.Cancelled) {
        <p-message 
          severity="warn" 
          text="Job was cancelled"
          styleClass="w-full mb-3">
        </p-message>
        
        <p-button
          label="Start New Job"
          icon="pi pi-refresh"
          (onClick)="startNewJob.emit()">
        </p-button>
      }

      <!-- Error Result -->
      @if (status === JobStatus.Failed) {
        <p-message 
          severity="error" 
          [text]="errorMessage || 'Job failed'"
          styleClass="w-full mb-3">
        </p-message>
        
        <p-button
          label="Try Again"
          icon="pi pi-refresh"
          (onClick)="startNewJob.emit()">
        </p-button>
      }
    </p-card>
  `,
  styles: [`
    .result-text-display {
      font-family: 'Courier New', Courier, monospace;
      font-size: 0.95rem;
      font-weight: 600;
      padding: 1rem;
      background: linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%);
      border: 2px solid var(--p-green-500, #10b981);
      border-radius: 6px;
      min-height: 150px;
      color: var(--p-green-900, #065f46);
      white-space: pre-wrap;
      word-wrap: break-word;
      overflow-y: auto;
      max-height: 300px;
    }

    .result-text-display::-webkit-scrollbar {
      width: 8px;
    }

    .result-text-display::-webkit-scrollbar-track {
      background: #a7f3d0;
    }

    .result-text-display::-webkit-scrollbar-thumb {
      background: #10b981;
      border-radius: 4px;
    }

    .result-text-display::-webkit-scrollbar-thumb:hover {
      background: #059669;
    }
  `]
})
export class JobResultComponent {
  @Input() status!: JobStatus;
  @Input() result: string = '';
  @Input() elapsedTime: string = '00:00';
  @Input() errorMessage: string | null = null;
  
  @Output() startNewJob = new EventEmitter<void>();
  
  JobStatus = JobStatus; // Expose to template
}