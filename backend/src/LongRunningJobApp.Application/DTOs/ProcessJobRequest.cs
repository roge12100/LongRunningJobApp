namespace LongRunningJobApp.Application.DTOs;

/// <summary>
/// Request to process a string
/// </summary>
public sealed record ProcessJobRequest
{
    public required string Input { get; init; }
}
