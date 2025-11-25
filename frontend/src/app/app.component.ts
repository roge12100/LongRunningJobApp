import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { JobInputComponent } from './components/job-input/job-input';
import { JobProgressComponent } from './components/job-progress/job-progress';
import { JobResultComponent } from './components/job-result/job-result';
import { JobService } from './services/job.service';
import { SignalrService } from './services/signalr.service';
import { 
  JobStatus, 
  CreateJobRequest,
  isActiveStatus,
  isTerminalStatus
} from './models/job.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    ToastModule,
    JobInputComponent,
    JobProgressComponent,
    JobResultComponent
  ],
  providers: [MessageService],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Long Running Job Processor';

  currentStatus = signal<JobStatus>(JobStatus.None);
  currentJobId = signal<string | null>(null);
  inputText = signal('');
  processedText = signal('');
  progress = signal(0);
  errorMessage = signal<string | null>(null);
  startTime = signal<Date | null>(null);
  elapsedTime = signal('00:00');

  canStartJob = computed(() => 
    this.inputText().trim().length > 0 && 
    this.currentStatus() !== JobStatus.Processing
  );

  isJobRunning = computed(() => 
    this.currentStatus() === JobStatus.Processing
  );

  isJobQueued = computed(() => 
    this.currentStatus() === JobStatus.Queued
  );

  isJobFinished = computed(() => 
    isTerminalStatus(this.currentStatus())
  );

  canCancelJob = computed(() => 
    isActiveStatus(this.currentStatus()) && 
    this.currentJobId() !== null
  );

  private subscriptions = new Subscription();
  private timerInterval?: any;

  JobStatus = JobStatus;

  constructor(
    private jobService: JobService,
    private signalRService: SignalrService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.setupSignalRListeners();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.stopTimer();
    this.cleanupConnection();
  }

  // Event Handlers
  async onStartJob(): Promise<void> {
    if (!this.canStartJob()) return;

    this.resetJobState();
    this.currentStatus.set(JobStatus.Queued);

    const request: CreateJobRequest = { input: this.inputText() };

    this.jobService.createJob(request).subscribe({
      next: async (response) => {
        this.currentJobId.set(response.jobId);
        this.currentStatus.set(response.status);
        this.startTime.set(new Date());
        this.startTimer();
        this.showSuccess('Job Created', 'Your job was created and queued for processing.');

        try {
          await this.signalRService.connect(response.hubUrl, response.jobId);
        } catch (err) {
          this.showError('Connection Error', 'Failed to connect to real-time updates');
        }
      },
      error: (err) => {
        this.currentStatus.set(JobStatus.Failed);
        this.errorMessage.set('Failed to create job. Please try again.');
        this.showError('Error', 'Failed to create job');
      }
    });
  }

  onCancelJob(): void {
    if (!this.canCancelJob() || !this.currentJobId()) return;

    this.jobService.cancelJob(this.currentJobId()!).subscribe({
      next: (response) => {
        if (response.success) {
          this.showWarning('Job cancellation in progress', response.message);
        }
        else{
          this.showError('Error', 'Failed to cancel job: ' + response.message);
        }
      },
      error: () => this.showError('Error', 'Failed to cancel job')
    });
  }

  onStartNewJob(): void {
    this.resetJobState();
    this.inputText.set('');
  }

  // SignalR Event Handlers
  private setupSignalRListeners(): void {
    this.subscriptions.add(
      this.signalRService.jobStarted$.subscribe((msg) => {
        console.log('JobStarted:', msg.jobId);
        this.currentStatus.set(JobStatus.Processing);
        this.showSuccess('Job Processing', 'Your job has now started processing...');
      })
    );

    this.subscriptions.add(
      this.signalRService.receiveCharacter$.subscribe((msg) => {
        console.log('ReceiveCharacter:', msg.character);
        this.processedText.update(text => text + msg.character);
      })
    );

    this.subscriptions.add(
      this.signalRService.progressUpdated$.subscribe((msg) => {
        console.log('ProgressUpdated:', msg.progressPercentage);
        this.progress.set(Math.round(msg.progressPercentage));
      })
    );

    this.subscriptions.add(
      this.signalRService.jobCompleted$.subscribe((msg) => {
        console.log('JobCompleted:', msg.jobId);
        this.currentStatus.set(JobStatus.Completed);
        this.processedText.set(msg.result);
        this.progress.set(100);
        this.stopTimer();
        this.showSuccess('Job Completed', 'Your job has been processed successfully!');
        this.cleanupConnection();
      })
    );

    this.subscriptions.add(
      this.signalRService.jobCancelled$.subscribe((msg) => {
        console.log('JobCancelled:', msg.jobId);
        this.currentStatus.set(JobStatus.Cancelled);
        this.stopTimer();
        this.showSuccess('Job Cancelled Successfully', 'The job was cancelled as requested.');
        this.resetJobState();
        this.cleanupConnection();
      })
    );

    this.subscriptions.add(
      this.signalRService.jobFailed$.subscribe((msg) => {
        console.log('JobFailed:', msg.jobId, msg.errorMessage);
        this.currentStatus.set(JobStatus.Failed);
        this.errorMessage.set(msg.errorMessage);
        this.stopTimer();
        this.showError('Job Failed', msg.errorMessage);
        this.cleanupConnection();
      })
    );
  }

  // Helper Methods
  private resetJobState(): void {
    this.currentStatus.set(JobStatus.None);
    this.currentJobId.set(null);
    this.processedText.set('');
    this.progress.set(0);
    this.errorMessage.set(null);
    this.startTime.set(null);
    this.elapsedTime.set('00:00');
    this.stopTimer();
  }

  private startTimer(): void {
    this.timerInterval = setInterval(() => {
      if (this.startTime()) {
        const elapsed = Date.now() - this.startTime()!.getTime();
        const seconds = Math.floor(elapsed / 1000);
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        this.elapsedTime.set(
          `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`
        );
      }
    }, 1000);
  }
  private async cleanupConnection(): Promise<void> {
    if (this.currentJobId()) {
      await this.signalRService.leaveGroup(this.currentJobId()!);
    }
    await this.signalRService.disconnect();
  }

  private stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = undefined;
    }
  }

  public getSubmitLabel(): string {
    if (this.isJobQueued()) {
      return 'Job Queued...';
    }
    else if (this.isJobRunning()) {
      return 'Job Running...';
    }
    return 'Start Job';
  }

  private showSuccess(summary: string, detail: string): void {
    this.messageService.add({ severity: 'success', summary, detail, life: 5000 });
  }

  private showError(summary: string, detail: string): void {
    this.messageService.add({ severity: 'error', summary, detail, sticky: true });
  }

  private showWarning(summary: string, detail: string): void {
    this.messageService.add({ severity: 'warn', summary, detail, life: 5000 });
  }
}