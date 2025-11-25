// Enums
export enum JobStatus {
    Queued = 0,  
    Processing = 1,
    Completed = 2,
    Cancelled = 3,
    Failed = 4,
    None = 5
}

// DTOs
export interface CreateJobRequest {
    input: string;
}

export interface CreateJobResponse {
    jobId: string;
    status: JobStatus;
    createdAt: string;
    hubUrl: string;
}

export interface CancelJobResponse {
    success: boolean;
    message: string;
}

// SignalR Hub Messages 
export interface JobStartedMessage {
    jobId: string;
}

export interface ReceiveCharacterMessage {
    character: string;
}

export interface JobCompletedMessage {
    jobId: string;
    result: string;
}

export interface JobCancelledMessage {
    jobId: string;
}

export interface JobFailedMessage {
    jobId: string;
    errorMessage: string;
}

export interface ProgressUpdatedMessage {
    progressPercentage: number;
}

// Utility Functions 
export function getJobStatusText(status: JobStatus): string {
    switch (status) {
        case JobStatus.Queued: return 'Queued';
        case JobStatus.Processing: return 'Processing';
        case JobStatus.Completed: return 'Completed';
        case JobStatus.Cancelled: return 'Cancelled';
        case JobStatus.Failed: return 'Failed';
        default: return 'Unknown';
    }
}

export function getJobStatusIcon(status: JobStatus): string {
    switch (status) {
        case JobStatus.Queued: return 'pi pi-clock';
        case JobStatus.Processing: return 'pi pi-spin pi-spinner';
        case JobStatus.Completed: return 'pi pi-check-circle';
        case JobStatus.Cancelled: return 'pi pi-times-circle';
        case JobStatus.Failed: return 'pi pi-exclamation-triangle';
        default: return 'pi pi-question-circle';
    }
}

export function getJobStatusSeverity(status: JobStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
        case JobStatus.Queued: return 'info';
        case JobStatus.Processing: return 'info';
        case JobStatus.Completed: return 'success';
        case JobStatus.Cancelled: return 'warn';
        case JobStatus.Failed: return 'danger';
        default: return 'secondary';
    }
}

export function isTerminalStatus(status: JobStatus): boolean {
    return status === JobStatus.Completed || status === JobStatus.Cancelled || status === JobStatus.Failed;
}

export function isActiveStatus(status: JobStatus): boolean {
    return status === JobStatus.Queued || status === JobStatus.Processing;
}