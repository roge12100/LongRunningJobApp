namespace LongRunningJobApp.Application.Interfaces;

public interface IConnectionsTrackerService
{
    void AddConnection(string jobId, string connectionId);
    
    void RemoveConnection(string connectionId);
    
    bool HasActiveConnections(string jobId);
    
    string? GetConnectionId(string jobId);
    
    string? GetJobIdByConnection(string connectionId);
    
}