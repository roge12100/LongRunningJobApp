using LongRunningJobApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LongRunningJobApp.Infrastructure.SignalR;

/// <summary>
/// Implementation of IJobProgressNotifier using SignalR
/// Sends real-time updates to clients via WebSocket
/// </summary>
public sealed class JobProgressNotifier : IJobProgressNotifier
{
    private readonly IHubContext<JobProgressHub> _hubContext;
    private readonly ILogger<JobProgressNotifier> _logger;

    public JobProgressNotifier(
        IHubContext<JobProgressHub> hubContext,
        ILogger<JobProgressNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyJobStartedAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notifying job started: {JobId}", jobId);
        
        await _hubContext.Clients
            .Group(jobId.ToString())
            .SendAsync("JobStarted", jobId, cancellationToken);
    }

    public async Task SendCharacterAsync(Guid jobId, string character, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(jobId.ToString())
            .SendAsync("ReceiveCharacter", character, cancellationToken);
    }

    public async Task NotifyJobCompletedAsync(Guid jobId, string result, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notifying job completed: {JobId}", jobId);
        
        await _hubContext.Clients
            .Group(jobId.ToString())
            .SendAsync("JobCompleted", jobId, result, cancellationToken);
    }

    public async Task NotifyJobCancelledAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notifying job cancelled: {JobId}", jobId);
        
        await _hubContext.Clients
            .Group(jobId.ToString())
            .SendAsync("JobCancelled", jobId, cancellationToken);
    }

    public async Task NotifyJobFailedAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default)
    {
        _logger.LogError("Notifying job failed: {JobId}, Error: {ErrorMessage}", jobId, errorMessage);
        
        await _hubContext.Clients
            .Group(jobId.ToString())
            .SendAsync("JobFailed", jobId, errorMessage, cancellationToken);
    }

    public async Task UpdateProgressAsync(Guid jobId, double progressPercentage, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(jobId.ToString())
            .SendAsync("ProgressUpdated", progressPercentage, cancellationToken);
    }
}
