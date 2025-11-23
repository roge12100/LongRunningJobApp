using System.Collections.Concurrent;
using System.Threading.Channels;
using LongRunningJobApp.Application.Interfaces;
using LongRunningJobApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LongRunningJobApp.Application.Services;

/// <summary>
/// Service for managing job lifecycle and operations
/// </summary>
public sealed class JobService : IJobService
{
    private readonly ConcurrentDictionary<Guid, JobInfo> _jobs;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokens;
    private readonly Channel<JobInfo> _jobQueue;
    private readonly ILogger<JobService> _logger;

    public JobService(ILogger<JobService> logger)
    {
        _logger = logger;
        _jobs = new ConcurrentDictionary<Guid, JobInfo>();
        _cancellationTokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        
        _jobQueue = Channel.CreateUnbounded<JobInfo>(new UnboundedChannelOptions
        {
            SingleReader = true, 
            SingleWriter = false  
        });
    }

    /// <summary>
    /// Gets the channel reader for background worker to consume jobs
    /// </summary>
    public ChannelReader<JobInfo> JobQueueReader => _jobQueue.Reader;

    /// <summary>
    /// Registers a cancellation token for a job
    /// </summary>
    public void RegisterCancellationToken(Guid jobId, CancellationTokenSource cts) 
        => _cancellationTokens.TryAdd(jobId, cts);
    

    /// <summary>
    /// Removes cancellation token after job completion
    /// </summary>
    public void UnregisterCancellationToken(Guid jobId)
    {
        if (_cancellationTokens.TryRemove(jobId, out var cts))
        {
            cts.Dispose();
        }
    }

    /// <summary>
    /// Creates and queues a new job for processing
    /// </summary>
    public async Task<JobInfo> CreateJobAsync(string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        var jobId = Guid.NewGuid();
        var job = new JobInfo(jobId, input);

        if (!_jobs.TryAdd(jobId, job))
        {
            _logger.LogError("Failed to add job {JobId} to store", jobId);
            throw new InvalidOperationException($"Failed to create job {jobId}");
        }

        _logger.LogInformation("Job {JobId} created with input length {InputLength}", 
            jobId, input.Length);

        await _jobQueue.Writer.WriteAsync(job, cancellationToken);

        _logger.LogInformation("Job {JobId} queued for processing", jobId);

        return job;
    }

    /// <summary>
    /// Gets job information by ID
    /// </summary>
    public JobInfo? GetJob(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return job;
    }

    /// <summary>
    /// Cancels a running or queued job
    /// </summary>
    public Task<bool> CancelJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            _logger.LogWarning("Attempted to cancel non-existent job {JobId}", jobId);
            return Task.FromResult(false);
        }

        if (!job.CanBeCancelled())
        {
            _logger.LogWarning("Job {JobId} cannot be cancelled. Current status: {Status}", 
                jobId, job.Status);
            return Task.FromResult(false);
        }

        if (_cancellationTokens.TryGetValue(jobId, out var cts))
        {
            _logger.LogInformation("Cancelling job {JobId}", jobId);
            cts.Cancel();
        }

        job.Cancel();
        
        _logger.LogInformation("Job {JobId} cancelled successfully", jobId);

        return Task.FromResult(true);
    }
    
}
