namespace LongRunningJobApp.Application.DTOs;

/// <summary>
/// Cancel job request
/// </summary>
public sealed record CancelJobRequest
{
    public required Guid JobId { get; init; }
}
