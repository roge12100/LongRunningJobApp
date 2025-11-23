using FluentAssertions;
using LongRunningJobApp.Application.Services;
using LongRunningJobApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace LongRunningJobApp.Application.Tests.Services;

public class JobServiceTests
{
    private readonly JobService _service;
    private readonly Mock<ILogger<JobService>> _loggerMock;

    public JobServiceTests()
    {
        _loggerMock = new Mock<ILogger<JobService>>();
        _service = new JobService(_loggerMock.Object);
    }

    [Fact]
    public async Task CreateJobAsync_WithValidInput_ShouldCreateJobInQueuedState()
    {
        // Arrange
        var input = "Hello, World!";

        // Act
        var job = await _service.CreateJobAsync(input);

        // Assert
        job.Should().NotBeNull();
        job.Id.Should().NotBeEmpty();
        job.Input.Should().Be(input);
        job.Status.Should().Be(JobStatus.Queued);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateJobAsync_WithInvalidInput_ShouldThrowArgumentException(string? invalidInput)
    {
        // Act
        var act = async () => await _service.CreateJobAsync(invalidInput!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateJobAsync_ShouldAddJobToStore()
    {
        // Arrange
        var input = "test";

        // Act
        var createdJob = await _service.CreateJobAsync(input);
        var retrievedJob = _service.GetJob(createdJob.Id);

        // Assert
        retrievedJob.Should().NotBeNull();
        retrievedJob.Should().BeSameAs(createdJob);
    }

    [Fact]
    public void GetJob_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var job = _service.GetJob(nonExistentId);

        // Assert
        job.Should().BeNull();
    }

    [Fact]
    public async Task CancelJobAsync_WithQueuedJob_ShouldReturnTrue()
    {
        // Arrange
        var job = await _service.CreateJobAsync("test");

        // Act
        var result = await _service.CancelJobAsync(job.Id);

        // Assert
        result.Should().BeTrue();
        job.Status.Should().Be(JobStatus.Cancelled);
    }

    [Fact]
    public async Task CancelJobAsync_WithNonExistentJob_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.CancelJobAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Failed)]
    public async Task CancelJobAsync_WithNonCancellableState_ShouldReturnFalse(JobStatus terminalState)
    {
        // Arrange
        var job = await _service.CreateJobAsync("test");
    
        if (terminalState == JobStatus.Completed)
        {
            job.MarkAsProcessing(10);
            job.Complete("result");
        }
        else if (terminalState == JobStatus.Failed)
        {
            job.MarkAsFailed("error");
        }

        // Act
        var result = await _service.CancelJobAsync(job.Id);

        // Assert
        result.Should().BeFalse();
        job.Status.Should().Be(terminalState); 
    }
    
}
