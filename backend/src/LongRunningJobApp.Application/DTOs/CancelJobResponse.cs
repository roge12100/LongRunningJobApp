namespace LongRunningJobApp.Application.DTOs;

/// <summary>
/// Cancel job response
/// </summary>
public sealed record CancelJobResponse
{
    public required bool Success { get; init; }
    
    public required string Message { get; init; }
    
}
