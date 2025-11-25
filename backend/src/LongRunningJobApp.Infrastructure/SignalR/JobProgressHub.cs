using LongRunningJobApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LongRunningJobApp.Infrastructure.SignalR;

/// <summary>
/// SignalR Hub for real-time job progress communication
/// Clients connect to this hub to receive updates about their jobs
/// </summary>
public sealed class JobProgressHub : Hub
{
    private readonly ILogger<JobProgressHub> _logger;
    private readonly IConnectionsTrackerService _connectionsTracker;
    private readonly INotificationService _notificationService;
    private readonly IJobService _jobService;

    public JobProgressHub(ILogger<JobProgressHub> logger, IConnectionsTrackerService connectionsTracker, INotificationService notificationService, IJobService jobService)
    {
        _logger = logger;
        _connectionsTracker = connectionsTracker;
        _notificationService = notificationService;
        _jobService = jobService;
    }

    /// <summary>
    /// Client joins a job group to receive updates for that specific job
    /// </summary>
    public async Task JoinJob(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        _connectionsTracker.AddConnection(jobId, Context.ConnectionId);
        _logger.LogInformation("Client {ConnectionId} joined job group {JobId}", 
            Context.ConnectionId, jobId);
        
        await _notificationService.FlushQueuedNotificationsAsync(jobId);
    }

    /// <summary>
    /// Client leaves a job group
    /// </summary>
    public async Task LeaveJob(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId);
        _logger.LogInformation("Client {ConnectionId} left job group {JobId}", 
            Context.ConnectionId, jobId);
        
        await _jobService.CancelJobAsync(Guid.Parse(jobId));
        _notificationService.ClearNotificationQueue(jobId);
        var connectionId = _connectionsTracker.GetConnectionId(jobId);
        _connectionsTracker.RemoveConnection(connectionId ?? "");
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        var jobId = _connectionsTracker.GetJobIdByConnection(Context.ConnectionId);
        
        await _jobService.CancelJobAsync(Guid.Parse(jobId));
        
        _notificationService.ClearNotificationQueue(jobId ?? "");
        
        _connectionsTracker.RemoveConnection(Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
}
