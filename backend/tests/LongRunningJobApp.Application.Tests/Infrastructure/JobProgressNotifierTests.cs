using LongRunningJobApp.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace LongRunningJobApp.Application.Tests.Infrastructure;

public class JobProgressNotifierTests
{
    private readonly Mock<IHubContext<JobProgressHub>> _hubContextMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<ILogger<JobProgressNotifier>> _loggerMock;
    private readonly JobProgressNotifier _service;

    public JobProgressNotifierTests()
    {
        _hubContextMock = new Mock<IHubContext<JobProgressHub>>();
        _clientProxyMock = new Mock<IClientProxy>();
        _loggerMock = new Mock<ILogger<JobProgressNotifier>>();

        _hubContextMock
            .Setup(x => x.Clients.Group(It.IsAny<string>()))
            .Returns(_clientProxyMock.Object);

        _service = new JobProgressNotifier(_hubContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendCharacterAsync_ShouldSendToCorrectGroup()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var character = "H";

        // Act
        await _service.SendCharacterAsync(jobId, character);

        // Assert
        _hubContextMock.Verify(
            x => x.Clients.Group(jobId.ToString()),
            Times.Once);

        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "ReceiveCharacter",
                It.Is<object[]>(args => args[0].ToString() == character),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyJobStartedAsync_ShouldSendToCorrectGroup()
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        await _service.NotifyJobStartedAsync(jobId);

        // Assert
        _hubContextMock.Verify(
            x => x.Clients.Group(jobId.ToString()),
            Times.Once);

        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "JobStarted",
                It.Is<object[]>(args => (Guid)args[0] == jobId),
                default),
            Times.Once);
    }

    [Fact]
    public async Task NotifyJobCompletedAsync_ShouldSendToCorrectGroup()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var result = "test result";

        // Act
        await _service.NotifyJobCompletedAsync(jobId, result);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "JobCompleted",
                It.Is<object[]>(args => (Guid)args[0] == jobId && args[1].ToString() == result),
                default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_ShouldSendProgressPercentage()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var progress = 45.5;

        // Act
        await _service.UpdateProgressAsync(jobId, progress);

        // Assert
        _clientProxyMock.Verify(
            x => x.SendCoreAsync(
                "ProgressUpdated",
                It.Is<object[]>(args => (double)args[0] == progress),
                default),
            Times.Once);
    }
}
