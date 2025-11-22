using LongRunningJobApp.Domain.Entities;

namespace LongRunningJobApp.Application.Interfaces;

/// <summary>
/// Service for managing job lifecycle and operations
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Creates and queues a new job for processing
    /// </summary>
    /// <param name="input">The input string to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created job information</returns>
    Task<JobInfo> CreateJobAsync(string input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets job information by ID
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <returns>Job information if found, null otherwise</returns>
    JobInfo? GetJob(Guid jobId);

    /// <summary>
    /// Cancels a running or queued job
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if cancelled, false if not found or cannot be cancelled</returns>
    Task<bool> CancelJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    
}
