namespace LongRunningJobApp.Application.DTOs;

/// <summary>
/// Request to process a string
/// </summary>
public sealed record CreateJobRequest
{
    public required string Input { get; init; }
}
