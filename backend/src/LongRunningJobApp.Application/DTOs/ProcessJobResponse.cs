using LongRunningJobApp.Domain.Enums;

namespace LongRunningJobApp.Application.DTOs;

/// <summary>
/// Response after creating a job
/// </summary>
public sealed record CreateJobResponse
{
    public required Guid JobId { get; init; }
    
    public required JobStatus Status { get; init; }
    
    public required DateTime CreatedAt { get; init; }
    
    public required string HubUrl { get; init; }

}
