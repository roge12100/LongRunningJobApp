using LongRunningJobApp.Domain.Enums;

namespace LongRunningJobApp.Application.DTOs;

/// <summary>
/// Job status response
/// </summary>
public sealed record JobStatusResponse
{
    public required Guid JobId { get; init; }
    
    public required string Input { get; init; }
    
    public required JobStatus Status { get; init; }
    
    public string? Result { get; init; }
    
    public required DateTime CreatedAt { get; init; }
    
    public DateTime? StartedAt { get; init; }
    
    public DateTime? CompletedAt { get; init; }
    
    public string? ErrorMessage { get; init; }
    
    public required int TotalCharacters { get; init; }
    
    public required int ProcessedCharacters { get; init; }
    
    public required double ProgressPercentage { get; init; }
    
}
