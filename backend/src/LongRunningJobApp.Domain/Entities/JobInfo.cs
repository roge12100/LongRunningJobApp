using LongRunningJobApp.Domain.Enums;
using LongRunningJobApp.Domain.Exceptions;

namespace LongRunningJobApp.Domain.Entities;

/// <summary>
/// Represents a long-running job with its state and business rules
/// </summary>
public sealed class JobInfo
{
    public Guid Id { get; init; }
    public string Input { get; init; }
    public string? Result { get; private set; }
    public JobStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int TotalCharacters { get; private set; }
    public int ProcessedCharacters { get; private set; }

    /// <summary>
    /// Creates a new job in Queued state
    /// </summary>
    public JobInfo(Guid id, string input)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Job ID cannot be empty", nameof(id));
        
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        Id = id;
        Input = input;
        Status = JobStatus.Queued;
        CreatedAt = DateTime.UtcNow;
        TotalCharacters = 0;
        ProcessedCharacters = 0;
    }

    /// <summary>
    /// Transitions job to Processing state
    /// </summary>
    public void MarkAsProcessing(int totalCharacters)
    {
        if (Status != JobStatus.Queued)
            throw new InvalidJobStateTransitionException(
                $"Cannot start processing job in {Status} state. Expected {JobStatus.Queued}");

        if (totalCharacters <= 0)
            throw new ArgumentException("Total characters must be greater than 0", nameof(totalCharacters));

        Status = JobStatus.Processing;
        StartedAt = DateTime.UtcNow;
        TotalCharacters = totalCharacters;
        ProcessedCharacters = 0;
    }

    /// <summary>
    /// Updates progress of character processing
    /// </summary>
    public void UpdateProgress(int processedCharacters)
    {
        if (Status != JobStatus.Processing)
            throw new InvalidJobStateTransitionException(
                $"Cannot update progress for job in {Status} state");

        if (processedCharacters < 0 || processedCharacters > TotalCharacters)
            throw new ArgumentOutOfRangeException(nameof(processedCharacters),
                $"Processed characters must be between 0 and {TotalCharacters}");

        ProcessedCharacters = processedCharacters;
    }

    /// <summary>
    /// Marks job as completed with final result
    /// </summary>
    public void Complete(string result)
    {
        if (Status != JobStatus.Processing)
            throw new InvalidJobStateTransitionException(
                $"Cannot complete job in {Status} state. Expected {JobStatus.Processing}");

        if (string.IsNullOrEmpty(result))
            throw new ArgumentException("Result cannot be null or empty", nameof(result));

        Status = JobStatus.Completed;
        Result = result;
        CompletedAt = DateTime.UtcNow;
        ProcessedCharacters = TotalCharacters;
    }

    /// <summary>
    /// Cancels the job
    /// </summary>
    public void Cancel()
    {
        if (Status == JobStatus.Completed)
            throw new InvalidJobStateTransitionException(
                "Cannot cancel a completed job");

        if (Status == JobStatus.Failed)
            throw new InvalidJobStateTransitionException(
                "Cannot cancel a failed job");

        if (Status == JobStatus.Cancelled)
            return; 

        Status = JobStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks job as failed with error message
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        if (Status == JobStatus.Completed)
            throw new InvalidJobStateTransitionException(
                "Cannot mark a completed job as failed");

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

        Status = JobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if job can be cancelled
    /// </summary>
    public bool CanBeCancelled()
        => Status is JobStatus.Queued or JobStatus.Processing;

    /// <summary>
    /// Gets progress percentage (0-100)
    /// </summary>
    public double GetProgressPercentage()
    {
        if (TotalCharacters == 0)
            return 0;

        return (double)ProcessedCharacters / TotalCharacters * 100;
    }

    /// <summary>
    /// Checks if job is in a terminal state
    /// </summary>
    public bool IsTerminal()
        => Status is JobStatus.Completed or JobStatus.Cancelled or JobStatus.Failed;
    
}