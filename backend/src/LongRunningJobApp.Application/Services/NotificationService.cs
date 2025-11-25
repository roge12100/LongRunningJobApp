using System.Collections.Concurrent;
using LongRunningJobApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LongRunningJobApp.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IJobProgressNotifier _progressNotifier;
    private readonly IConnectionsTrackerService _connectionsTracker;
    private readonly ILogger<NotificationService> _logger;
    
    private readonly ConcurrentDictionary<string, ConcurrentQueue<Func<Task>>> _notificationQueues = new();

    public NotificationService(
        IJobProgressNotifier progressNotifier,
        IConnectionsTrackerService connectionsTracker,
        ILogger<NotificationService> logger)
    {
        _progressNotifier = progressNotifier;
        _connectionsTracker = connectionsTracker;
        _logger = logger;
    }

    public async Task NotifyAsync(string jobId, Func<IJobProgressNotifier, Task> notificationAction)
    {
        if (_connectionsTracker.HasActiveConnections(jobId))
        {
            try
            {
                await notificationAction(_progressNotifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending real-time notification for job {JobId}", jobId);
            }
        }
        else
        {
            EnqueueNotification(jobId, () => notificationAction(_progressNotifier));
            _logger.LogInformation("No active connections for job {JobId}, notification queued", jobId);
        }
    }

    public async Task FlushQueuedNotificationsAsync(string jobId)
    {
        if (!_notificationQueues.TryGetValue(jobId, out var queue))
        {
            _logger.LogInformation("No queued notifications for job {JobId}", jobId);
            return;
        }

        var count = 0;
        while (queue.TryDequeue(out var action))
        {
            try
            {
                await action();
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing queued notification for job {JobId}", jobId);
            }
        }

        _logger.LogInformation("Flushed {Count} queued notifications for job {JobId}", count, jobId);
        
        if (queue.IsEmpty)
        {
            _notificationQueues.TryRemove(jobId, out _);
        }
    }

    private void EnqueueNotification(string jobId, Func<Task> notificationAction)
    {
        var queue = _notificationQueues.GetOrAdd(jobId, _ => new ConcurrentQueue<Func<Task>>());
        queue.Enqueue(notificationAction);
    }
    
    public void ClearNotificationQueue(string jobId)
    {
        _notificationQueues.TryRemove(jobId, out _);
        _logger.LogInformation("Cleared notification queue for job {JobId}", jobId);
    }
}