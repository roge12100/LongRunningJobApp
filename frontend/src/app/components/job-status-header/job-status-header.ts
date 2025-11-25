import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { JobStatus, getJobStatusText, getJobStatusIcon, getJobStatusSeverity } from '../../models/job.model';

@Component({
  selector: 'app-job-status-header',
  standalone: true,
  imports: [CommonModule, TagModule],
  template: `
    <div class="flex align-items-center justify-content-between p-3">
      <div class="flex align-items-center gap-2">
        <i [class]="getIcon()" [style.fontSize]="'1.5rem'" [style.color]="iconColor"></i>
        <span class="text-xl font-semibold" [style.color]="textColor">{{ getText() }}</span>
      </div>
      <p-tag 
        [value]="getText()"
        [severity]="getSeverity()"
        [icon]="getIcon()">
      </p-tag>
    </div>
  `
})
export class JobStatusHeaderComponent {
  @Input() status!: JobStatus;
  @Input() iconColor?: string;
  @Input() textColor?: string;

  getText(): string {
    return getJobStatusText(this.status);
  }

  getIcon(): string {
    return getJobStatusIcon(this.status);
  }

  getSeverity(): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    return getJobStatusSeverity(this.status);
  }
}