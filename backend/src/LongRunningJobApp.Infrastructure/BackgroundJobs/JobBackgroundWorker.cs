using LongRunningJobApp.Application.Interfaces;
using LongRunningJobApp.Application.Services;
using LongRunningJobApp.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LongRunningJobApp.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service that processes jobs from the channel internal queue
/// Streams results character-by-character with random delays
/// </summary>
public sealed class JobBackgroundWorker : BackgroundService
{
    private readonly JobService _jobService;
    private readonly IStringProcessor _stringProcessor;
    private readonly ILogger<JobBackgroundWorker> _logger;
    private readonly INotificationService _notificationService;

    public JobBackgroundWorker(
        JobService jobService,
        IStringProcessor stringProcessor,
        ILogger<JobBackgroundWorker> logger,
        INotificationService notificationService)
    {
        _jobService = jobService;
        _stringProcessor = stringProcessor;
        _logger = logger;
        _notificationService = notificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Background Worker started");

        try
        {
            await foreach (var job in _jobService.JobQueueReader.ReadAllAsync(stoppingToken))
            {
                Task.Run(() => ProcessJobAsync(job, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job Background Worker is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Job Background Worker");
        }
    }

    private async Task ProcessJobAsync(JobInfo job, CancellationToken stoppingToken)
    {
        using var jobCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _jobService.RegisterCancellationToken(job.Id, jobCts);

        try
        {
            _logger.LogInformation("Starting to process job {JobId}", job.Id);

            var result = _stringProcessor.Process(job.Input);

            job.MarkAsProcessing(result.Length);
            await _notificationService.NotifyAsync(job.Id.ToString(), notifier => notifier.NotifyJobStartedAsync(job.Id, jobCts.Token));
            
            for (int i = 0; i < result.Length; i++)
            {
                jobCts.Token.ThrowIfCancellationRequested();

                var character = result[i].ToString();

                var delayMs = Random.Shared.Next(1000, 5001);
                await Task.Delay(delayMs, jobCts.Token);
                
                await _notificationService.NotifyAsync(job.Id.ToString(), notifier => notifier.SendCharacterAsync(job.Id, character, jobCts.Token));
                job.UpdateProgress(i + 1);
                var progressPercentage = job.GetProgressPercentage();
                await _notificationService.NotifyAsync(job.Id.ToString(), notifier => notifier.UpdateProgressAsync(job.Id, progressPercentage, jobCts.Token));

                _logger.LogDebug("Job {JobId}: Sent character {Index}/{Total} ({Progress}%)",
                    job.Id, i + 1, result.Length, progressPercentage);
            }

            job.Complete(result);
            await _notificationService.NotifyAsync(job.Id.ToString(), notifier => notifier.NotifyJobCompletedAsync(job.Id, result, jobCts.Token));
            
            _logger.LogInformation("Job {JobId} completed successfully", job.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job {JobId} was cancelled", job.Id);
            
            if (!job.IsTerminal())
                job.Cancel();
            await Task.Delay(300); // Small delay to ensure client receives CancelJob endpoint response first
            await _notificationService.NotifyAsync(job.Id.ToString(), notifier => notifier.NotifyJobCancelledAsync(job.Id, stoppingToken));
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed with error", job.Id);
            
            var errorMessage = $"Job processing failed: {ex.Message}";
            job.MarkAsFailed(errorMessage);
            await _notificationService.NotifyAsync(job.Id.ToString(), notifier => notifier.NotifyJobFailedAsync(job.Id, errorMessage, stoppingToken));
            
        }
        finally
        {
            _jobService.UnregisterCancellationToken(job.Id);
        }
    }
}
