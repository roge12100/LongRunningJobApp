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

    public JobProgressHub(ILogger<JobProgressHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Client joins a job group to receive updates for that specific job
    /// </summary>
    public async Task JoinJob(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        _logger.LogInformation("Client {ConnectionId} joined job group {JobId}", 
            Context.ConnectionId, jobId);
    }

    /// <summary>
    /// Client leaves a job group
    /// </summary>
    public async Task LeaveJob(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId);
        _logger.LogInformation("Client {ConnectionId} left job group {JobId}", 
            Context.ConnectionId, jobId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
