import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject, BehaviorSubject } from 'rxjs';
import { 
  JobStartedMessage,
  ReceiveCharacterMessage,
  JobCompletedMessage,
  JobCancelledMessage,
  JobFailedMessage,
  ProgressUpdatedMessage
} from '../models/job.model';

export type ConnectionEvent = 'reconnecting' | 'reconnected' | 'failed';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection?: HubConnection;
  
  // Observables for SignalR events
  public jobStarted$ = new Subject<JobStartedMessage>();
  public receiveCharacter$ = new Subject<ReceiveCharacterMessage>();
  public jobCompleted$ = new Subject<JobCompletedMessage>();
  public jobCancelled$ = new Subject<JobCancelledMessage>();
  public jobFailed$ = new Subject<JobFailedMessage>();
  public progressUpdated$ = new Subject<ProgressUpdatedMessage>();
  
  public connectionStatus$ = new BehaviorSubject<boolean>(false);
  public connectionId$ = new BehaviorSubject<string | null>(null);
  public connectionEvent$ = new Subject<ConnectionEvent>();

  private isJobActive = false;

  constructor() {}

  /**
   * Connect to SignalR hub and join job group
   */
  public async connect(hubUrl: string, jobId: string): Promise<void> {
    try {
      this.isJobActive = true;

      this.hubConnection = new HubConnectionBuilder()
        .withUrl(hubUrl, {timeout: 10000,})        
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect([0, 2000, 5000])
        .build();

      this.setupConnectionHandlers();
      this.registerHandlers();

      await this.hubConnection.start();
      console.log('SignalR Connected');
      this.connectionStatus$.next(true);
      
      const connId = this.hubConnection.connectionId;
      this.connectionId$.next(connId ?? null);
      console.log('Connection ID:', connId);

      await this.joinGroup(jobId);
      
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      this.connectionStatus$.next(false);
      throw err;
    }
  }

  private setupConnectionHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR Reconnecting...', error);
      this.connectionStatus$.next(false);
      this.connectionEvent$.next('reconnecting');
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR Reconnected', connectionId);
      this.connectionStatus$.next(true);
      this.connectionId$.next(connectionId ?? null);
      this.connectionEvent$.next('reconnected');
    });

    this.hubConnection.onclose(() => {
      if (this.isJobActive) { 
        this.connectionEvent$.next('failed');
        this.isJobActive = false;
      }
    });
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('JobStarted', (jobId: string) => {
      console.log('JobStarted:', jobId);
      this.jobStarted$.next({ jobId });
    });

    this.hubConnection.on('ReceiveCharacter', (character: string) => {
      console.log('ReceiveCharacter:', character);
      this.receiveCharacter$.next({ character });
    });

    this.hubConnection.on('JobCompleted', (jobId: string, result: string) => {
      console.log('JobCompleted:', jobId);
      this.jobCompleted$.next({ jobId, result });
    });

    this.hubConnection.on('JobCancelled', (jobId: string) => {
      console.log('JobCancelled:', jobId);
      this.jobCancelled$.next({ jobId });
    });

    this.hubConnection.on('JobFailed', (jobId: string, errorMessage: string) => {
      console.log('JobFailed:', jobId, errorMessage);
      this.jobFailed$.next({ jobId, errorMessage });
    });

    this.hubConnection.on('ProgressUpdated', (progressPercentage: number) => {
      console.log('ProgressUpdated:', progressPercentage);
      this.progressUpdated$.next({ progressPercentage });
    });
  }

  private async joinGroup(jobId: string): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke('JoinJob', jobId);
        console.log('Joined job group:', jobId);
      } catch (err) {
        console.error('Failed to join job group:', err);
        throw err;
      }
    }
  }

  public async leaveGroup(jobId: string): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke('LeaveJob', jobId);
        console.log('Left job group:', jobId);
      } catch (err) {
        console.error('Failed to leave job group:', err);
      }
    }
  }

  public async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connectionStatus$.next(false);
      this.connectionId$.next(null);
      console.log('SignalR Disconnected');
    }
  }

  public getConnectionId(): string | null {
    return this.hubConnection?.connectionId ?? null;
  }
}