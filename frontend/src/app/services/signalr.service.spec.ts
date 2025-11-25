import { TestBed } from '@angular/core/testing';
import { SignalrService } from './signalr.service';
import { firstValueFrom } from 'rxjs';

describe('SignalrService', () => {
  let service: SignalrService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SignalrService]
    });
    
    service = TestBed.inject(SignalrService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start with disconnected status', async () => {
    const status = await firstValueFrom(service.connectionStatus$);
    expect(status).toBe(false);
  });

  it('should start with null connection id', async () => {
    const id = await firstValueFrom(service.connectionId$);
    expect(id).toBeNull();
  });

  it('should have all required observables', () => {
    expect(service.jobStarted$).toBeDefined();
    expect(service.receiveCharacter$).toBeDefined();
    expect(service.jobCompleted$).toBeDefined();
    expect(service.jobCancelled$).toBeDefined();
    expect(service.jobFailed$).toBeDefined();
    expect(service.progressUpdated$).toBeDefined();
  });
});