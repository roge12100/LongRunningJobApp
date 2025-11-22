namespace LongRunningJobApp.Domain.Enums;

/// <summary>
/// Represents the lifecycle states of a job
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job has been created and queued for processing
    /// </summary>
    Queued = 0,
    
    /// <summary>
    /// Job is currently being processed
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Job completed successfully
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// Job was cancelled by user or system
    /// </summary>
    Cancelled = 3,
    
    /// <summary>
    /// Job failed due to an error
    /// </summary>
    Failed = 4
}
