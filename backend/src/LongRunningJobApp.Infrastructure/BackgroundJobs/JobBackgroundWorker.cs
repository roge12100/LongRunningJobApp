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
    private readonly IJobProgressNotifier _progressNotifier;
    private readonly ILogger<JobBackgroundWorker> _logger;

    public JobBackgroundWorker(
        JobService jobService,
        IStringProcessor stringProcessor,
        IJobProgressNotifier progressNotifier,
        ILogger<JobBackgroundWorker> logger)
    {
        _jobService = jobService;
        _stringProcessor = stringProcessor;
        _progressNotifier = progressNotifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Background Worker started");

        try
        {
            await foreach (var job in _jobService.JobQueueReader.ReadAllAsync(stoppingToken))
            {
                await ProcessJobAsync(job, stoppingToken);
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
            await Task.Delay(1000);
            await _progressNotifier.NotifyJobStartedAsync(job.Id, jobCts.Token);

            for (int i = 0; i < result.Length; i++)
            {
                jobCts.Token.ThrowIfCancellationRequested();

                var character = result[i].ToString();

                var delayMs = Random.Shared.Next(1000, 5001);
                await Task.Delay(delayMs, jobCts.Token);

                await _progressNotifier.SendCharacterAsync(job.Id, character, jobCts.Token);

                job.UpdateProgress(i + 1);
                var progressPercentage = job.GetProgressPercentage();
                await _progressNotifier.UpdateProgressAsync(job.Id, progressPercentage, jobCts.Token);

                _logger.LogDebug("Job {JobId}: Sent character {Index}/{Total} ({Progress}%)",
                    job.Id, i + 1, result.Length, progressPercentage);
            }

            job.Complete(result);
            await _progressNotifier.NotifyJobCompletedAsync(job.Id, result, jobCts.Token);

            _logger.LogInformation("Job {JobId} completed successfully", job.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job {JobId} was cancelled", job.Id);
            
            if (!job.IsTerminal())
                job.Cancel();
            await _progressNotifier.NotifyJobCancelledAsync(job.Id, stoppingToken);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed with error", job.Id);
            
            var errorMessage = $"Job processing failed: {ex.Message}";
            job.MarkAsFailed(errorMessage);
            await _progressNotifier.NotifyJobFailedAsync(job.Id, errorMessage, stoppingToken);
        }
        finally
        {
            _jobService.UnregisterCancellationToken(job.Id);
        }
    }
}
