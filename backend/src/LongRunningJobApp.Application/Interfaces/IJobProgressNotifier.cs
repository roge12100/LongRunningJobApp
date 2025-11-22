namespace LongRunningJobApp.Application.Interfaces;

/// <summary>
/// Service for notifying clients about job progress via real-time communication
/// </summary>
public interface IJobProgressNotifier
{
    /// <summary>
    /// Notifies that a job has started processing
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task NotifyJobStartedAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a processed character to the client
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="character">The character to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendCharacterAsync(Guid jobId, string character, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a job has completed successfully
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="result">The complete result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task NotifyJobCompletedAsync(Guid jobId, string result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a job was cancelled
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task NotifyJobCancelledAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies that a job has failed
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task NotifyJobFailedAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates job progress percentage
    /// </summary>
    /// <param name="jobId">The job identifier</param>
    /// <param name="progressPercentage">Progress percentage (0-100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateProgressAsync(Guid jobId, double progressPercentage, CancellationToken cancellationToken = default);
    
}
