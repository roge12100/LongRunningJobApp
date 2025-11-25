using FluentAssertions;
using LongRunningJobApp.Api.Controllers;
using LongRunningJobApp.Application.DTOs;
using LongRunningJobApp.Application.Interfaces;
using LongRunningJobApp.Domain.Entities;
using LongRunningJobApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LongRunningJobApp.Api.Tests.Controllers;

public class JobsControllerTests
{
    private readonly Mock<IJobService> _jobServiceMock;
    private readonly Mock<ILogger<JobsController>> _loggerMock;
    private readonly JobsController _controller;

    public JobsControllerTests()
    {
        _jobServiceMock = new Mock<IJobService>();
        _loggerMock = new Mock<ILogger<JobsController>>();
        _controller = new JobsController(_jobServiceMock.Object, _loggerMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "http",
                    Host = new HostString("localhost:5000")
                }
            }
        };
    }

    [Fact]
    public async Task CreateJob_WithValidInput_ShouldReturnAcceptedWithResponse()
    {
        // Arrange
        var request = new CreateJobRequest { Input = "Hello, World!" };
        var jobId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var job = new JobInfo(jobId, request.Input);

        _jobServiceMock
            .Setup(x => x.CreateJobAsync(request.Input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var result = await _controller.CreateJob(request, CancellationToken.None);

        // Assert
        var acceptedResult = result.Result.Should().BeOfType<AcceptedResult>().Subject;
        var response = acceptedResult.Value.Should().BeOfType<CreateJobResponse>().Subject;
        
        response.JobId.Should().Be(jobId);
        response.Status.Should().Be(JobStatus.Queued);
        response.HubUrl.Should().Be("http://localhost:5000/hub/job-progress");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateJob_WithInvalidInput_ShouldReturnBadRequest(string? invalidInput)
    {
        // Arrange
        var request = new CreateJobRequest { Input = invalidInput! };

        // Act
        var result = await _controller.CreateJob(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateJob_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var request = new CreateJobRequest { Input = "test" };
        _jobServiceMock
            .Setup(x => x.CreateJobAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.CreateJob(request, CancellationToken.None);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CancelJob_WithExistingJob_ShouldReturnOkWithSuccess()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new JobInfo(jobId, "test");

        _jobServiceMock.Setup(x => x.GetJob(jobId)).Returns(job);
        _jobServiceMock.Setup(x => x.CancelJobAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelJob(jobId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CancelJobResponse>().Subject;
        
        response.Success.Should().BeTrue();
        response.Message.Should().Contain("Job cancellation request accepted");
    }

    [Fact]
    public async Task CancelJob_WithNonExistentJob_ShouldReturnNotFound()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _jobServiceMock.Setup(x => x.GetJob(jobId)).Returns((JobInfo?)null);

        // Act
        var result = await _controller.CancelJob(jobId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CancelJob_WhenJobCannotBeCancelled_ShouldReturnOkWithFailure()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new JobInfo(jobId, "test");
        job.MarkAsProcessing(10);
        job.Complete("result");

        _jobServiceMock.Setup(x => x.GetJob(jobId)).Returns(job);
        _jobServiceMock.Setup(x => x.CancelJobAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CancelJob(jobId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CancelJobResponse>().Subject;
        
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("cannot be cancelled");
    }
}
