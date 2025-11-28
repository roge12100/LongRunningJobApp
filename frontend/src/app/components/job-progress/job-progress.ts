import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { JobStatus } from '../../models/job.model';
import { JobStatusHeaderComponent } from '../job-status-header/job-status-header';
import { CustomProgressBarComponent } from '../custom-progress-bar/custom-progress-bar.component';

@Component({
  selector: 'app-job-progress',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    CustomProgressBarComponent,
    JobStatusHeaderComponent
  ],
  template: `
    <p-card styleClass="mb-3 fade-in">
      <ng-template pTemplate="header">
        <app-job-status-header [status]="status"></app-job-status-header>
      </ng-template>

      <div class="progress-section">
        <!-- Progress Bar -->
        <div class="mb-3">
          <label class="block mb-2 font-semibold">Progress</label>
          <app-custom-progress-bar [value]="progress" [showPercentage]="true" [animated]="true" class="mb-2"></app-custom-progress-bar>
          <div class="flex justify-content-between text-sm text-color-secondary">
            <span>Elapsed: {{ elapsedTime }}</span>
          </div>
        </div>

        <!-- Real-time Processed Text -->
        <div class="mb-3">
          <label class="block mb-2 font-semibold">
            Processed Text (Real-time)
            <i class="pi pi-spin pi-spinner ml-2" style="font-size: 0.8rem;"></i>
          </label>
          <div class="processed-text-display">
            {{ processedText || 'Waiting for data...' }}
          </div>
          <small class="text-color-secondary">
            Receiving characters in real-time...
          </small>
        </div>

        <!-- Cancel Button -->
        <p-button
          label="Cancel Job"
          icon="pi pi-times"
          severity="danger"
          [disabled]="!canCancel"
          (onClick)="cancelJob.emit()">
        </p-button>
      </div>
    </p-card>
  `,
  styles: [`
    .processed-text-display {
      font-family: 'Courier New', Courier, monospace;
      font-size: 0.95rem;
      padding: 1rem;
      background-color: var(--p-surface-100, #f8f9fa);
      border: 1px solid var(--p-surface-300, #dee2e6);
      border-radius: 6px;
      min-height: 150px;
      white-space: pre-wrap;
      word-wrap: break-word;
      overflow-y: auto;
      max-height: 300px;
    }

    .processed-text-display::-webkit-scrollbar {
      width: 8px;
    }

    .processed-text-display::-webkit-scrollbar-track {
      background: #f1f1f1;
    }

    .processed-text-display::-webkit-scrollbar-thumb {
      background: #888;
      border-radius: 4px;
    }

    .processed-text-display::-webkit-scrollbar-thumb:hover {
      background: #555;
    }
  `]
})
export class JobProgressComponent {
  @Input() status!: JobStatus;
  @Input() progress: number = 0;
  @Input() processedText: string = '';
  @Input() elapsedTime: string = '00:00';
  @Input() canCancel: boolean = true;
  
  @Output() cancelJob = new EventEmitter<void>();
}