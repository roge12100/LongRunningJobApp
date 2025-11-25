using System.Collections.Concurrent;
using LongRunningJobApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LongRunningJobApp.Application.Services;

public class HubConnectionsTrackerService : IConnectionsTrackerService
{
    private readonly ConcurrentDictionary<string, string> _hubConnections = new();
    private readonly ILogger<HubConnectionsTrackerService> _logger;

    public HubConnectionsTrackerService(ILogger<HubConnectionsTrackerService> logger)
    {
        _logger = logger;
    }

    public void AddConnection(string jobId, string connectionId)
    {
        _hubConnections.AddOrUpdate(
            jobId, 
            connectionId, 
            (_, oldConnectionId) =>
            {
                _logger.LogWarning("Replacing connection {OldConnectionId} with {NewConnectionId} for job {JobId}",
                    oldConnectionId, connectionId, jobId);
                return connectionId;
            });
    }

    public void RemoveConnection(string jobId)
        => _hubConnections.TryRemove(jobId, out _);

    public bool HasActiveConnections(string jobId)
        => _hubConnections.ContainsKey(jobId);
    
    public string? GetConnectionId(string jobId) 
        => _hubConnections.GetValueOrDefault(jobId);
    
    public string? GetJobIdByConnection(string connectionId)
    {
        foreach (var kvp in _hubConnections)
        {
            if (kvp.Value == connectionId)
            {
                return kvp.Key;
            }
        }
        return null;
    }
    
}