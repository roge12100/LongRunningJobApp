namespace LongRunningJobApp.Application.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(string jobId, Func<IJobProgressNotifier, Task> notificationAction);
    
    Task FlushQueuedNotificationsAsync(string jobId);
    
    void ClearNotificationQueue(string jobId);
    
}