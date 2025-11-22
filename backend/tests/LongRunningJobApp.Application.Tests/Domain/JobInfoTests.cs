using FluentAssertions;
using LongRunningJobApp.Domain.Entities;
using LongRunningJobApp.Domain.Enums;
using LongRunningJobApp.Domain.Exceptions;

namespace LongRunningJobApp.Application.Tests.Domain;

public class JobInfoTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateJobInQueuedState()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var input = "Hello, World!";

        // Act
        var job = new JobInfo(jobId, input);

        // Assert
        job.Id.Should().Be(jobId);
        job.Input.Should().Be(input);
        job.Status.Should().Be(JobStatus.Queued);
        job.Result.Should().BeNull();
        job.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        job.StartedAt.Should().BeNull();
        job.CompletedAt.Should().BeNull();
        job.ErrorMessage.Should().BeNull();
        job.TotalCharacters.Should().Be(0);
        job.ProcessedCharacters.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var input = "test";

        // Act
        var act = () => new JobInfo(emptyGuid, input);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Job ID cannot be empty*")
            .And.ParamName.Should().Be("id");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidInput_ShouldThrowArgumentException(string? invalidInput)
    {
        // Arrange
        var jobId = Guid.NewGuid();

        // Act
        var act = () => new JobInfo(jobId, invalidInput!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Input cannot be null or empty*")
            .And.ParamName.Should().Be("input");
    }

    #endregion

    #region MarkAsProcessing Tests

    [Fact]
    public void MarkAsProcessing_FromQueuedState_ShouldTransitionToProcessing()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        var totalCharacters = 50;

        // Act
        job.MarkAsProcessing(totalCharacters);

        // Assert
        job.Status.Should().Be(JobStatus.Processing);
        job.StartedAt.Should().NotBeNull();
        job.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        job.TotalCharacters.Should().Be(totalCharacters);
        job.ProcessedCharacters.Should().Be(0);
    }

    [Theory]
    [InlineData(JobStatus.Processing)]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Cancelled)]
    [InlineData(JobStatus.Failed)]
    public void MarkAsProcessing_FromNonQueuedState_ShouldThrowInvalidJobStateTransitionException(JobStatus invalidStatus)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (invalidStatus == JobStatus.Processing)
        {
            job.MarkAsProcessing(10);
        }
        else if (invalidStatus == JobStatus.Completed)
        {
            job.MarkAsProcessing(10);
            job.Complete("result");
        }
        else if (invalidStatus == JobStatus.Cancelled)
        {
            job.Cancel();
        }
        else if (invalidStatus == JobStatus.Failed)
        {
            job.MarkAsFailed("error");
        }

        // Act
        var act = () => job.MarkAsProcessing(20);

        // Assert
        act.Should().Throw<InvalidJobStateTransitionException>()
            .WithMessage($"*Cannot start processing job in {invalidStatus} state*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void MarkAsProcessing_WithInvalidTotalCharacters_ShouldThrowArgumentException(int invalidTotal)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");

        // Act
        var act = () => job.MarkAsProcessing(invalidTotal);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Total characters must be greater than 0*")
            .And.ParamName.Should().Be("totalCharacters");
    }

    #endregion

    #region UpdateProgress Tests

    [Fact]
    public void UpdateProgress_InProcessingState_ShouldUpdateProcessedCharacters()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(100);

        // Act
        job.UpdateProgress(25);

        // Assert
        job.ProcessedCharacters.Should().Be(25);
        job.Status.Should().Be(JobStatus.Processing);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void UpdateProgress_WithValidValues_ShouldSucceed(int processedCount)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(100);

        // Act
        job.UpdateProgress(processedCount);

        // Assert
        job.ProcessedCharacters.Should().Be(processedCount);
    }

    [Theory]
    [InlineData(JobStatus.Queued)]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Cancelled)]
    [InlineData(JobStatus.Failed)]
    public void UpdateProgress_InNonProcessingState_ShouldThrowInvalidJobStateTransitionException(JobStatus invalidStatus)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (invalidStatus == JobStatus.Completed)
        {
            job.MarkAsProcessing(10);
            job.Complete("result");
        }
        else if (invalidStatus == JobStatus.Cancelled)
        {
            job.Cancel();
        }
        else if (invalidStatus == JobStatus.Failed)
        {
            job.MarkAsFailed("error");
        }

        // Act
        var act = () => job.UpdateProgress(5);

        // Assert
        act.Should().Throw<InvalidJobStateTransitionException>()
            .WithMessage($"*Cannot update progress for job in {invalidStatus} state*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void UpdateProgress_WithOutOfRangeValue_ShouldThrowArgumentOutOfRangeException(int invalidProgress)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(100);

        // Act
        var act = () => job.UpdateProgress(invalidProgress);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("processedCharacters");
    }

    #endregion

    #region Complete Tests

    [Fact]
    public void Complete_FromProcessingState_ShouldTransitionToCompleted()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(50);
        var result = " 1e1s1t2/dGVzdA==";

        // Act
        job.Complete(result);

        // Assert
        job.Status.Should().Be(JobStatus.Completed);
        job.Result.Should().Be(result);
        job.CompletedAt.Should().NotBeNull();
        job.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        job.ProcessedCharacters.Should().Be(job.TotalCharacters);
    }

    [Theory]
    [InlineData(JobStatus.Queued)]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Cancelled)]
    [InlineData(JobStatus.Failed)]
    public void Complete_FromNonProcessingState_ShouldThrowInvalidJobStateTransitionException(JobStatus invalidStatus)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (invalidStatus == JobStatus.Completed)
        {
            job.MarkAsProcessing(10);
            job.Complete("first");
        }
        else if (invalidStatus == JobStatus.Cancelled)
        {
            job.Cancel();
        }
        else if (invalidStatus == JobStatus.Failed)
        {
            job.MarkAsFailed("error");
        }

        // Act
        var act = () => job.Complete("result");

        // Assert
        act.Should().Throw<InvalidJobStateTransitionException>()
            .WithMessage($"*Cannot complete job in {invalidStatus} state*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Complete_WithInvalidResult_ShouldThrowArgumentException(string? invalidResult)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(10);

        // Act
        var act = () => job.Complete(invalidResult!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Result cannot be null or empty*")
            .And.ParamName.Should().Be("result");
    }

    #endregion

    #region Cancel Tests

    [Theory]
    [InlineData(JobStatus.Queued)]
    [InlineData(JobStatus.Processing)]
    public void Cancel_FromCancellableState_ShouldTransitionToCancelled(JobStatus initialState)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (initialState == JobStatus.Processing)
        {
            job.MarkAsProcessing(10);
        }

        // Act
        job.Cancel();

        // Assert
        job.Status.Should().Be(JobStatus.Cancelled);
        job.CompletedAt.Should().NotBeNull();
        job.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldBeIdempotent()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.Cancel();
        var firstCancelTime = job.CompletedAt;

        // Act
        job.Cancel();

        // Assert
        job.Status.Should().Be(JobStatus.Cancelled);
        job.CompletedAt.Should().Be(firstCancelTime);
    }

    [Fact]
    public void Cancel_FromCompletedState_ShouldThrowInvalidJobStateTransitionException()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(10);
        job.Complete("result");

        // Act
        var act = () => job.Cancel();

        // Assert
        act.Should().Throw<InvalidJobStateTransitionException>()
            .WithMessage("*Cannot cancel a completed job*");
    }

    [Fact]
    public void Cancel_FromFailedState_ShouldThrowInvalidJobStateTransitionException()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsFailed("error occurred");

        // Act
        var act = () => job.Cancel();

        // Assert
        act.Should().Throw<InvalidJobStateTransitionException>()
            .WithMessage("*Cannot cancel a failed job*");
    }

    #endregion

    #region MarkAsFailed Tests

    [Theory]
    [InlineData(JobStatus.Queued)]
    [InlineData(JobStatus.Processing)]
    [InlineData(JobStatus.Cancelled)]
    public void MarkAsFailed_FromNonCompletedState_ShouldTransitionToFailed(JobStatus initialState)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        var errorMessage = "Something went wrong";
        
        if (initialState == JobStatus.Processing)
        {
            job.MarkAsProcessing(10);
        }
        else if (initialState == JobStatus.Cancelled)
        {
            job.Cancel();
        }

        // Act
        job.MarkAsFailed(errorMessage);

        // Assert
        job.Status.Should().Be(JobStatus.Failed);
        job.ErrorMessage.Should().Be(errorMessage);
        job.CompletedAt.Should().NotBeNull();
        job.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsFailed_FromCompletedState_ShouldThrowInvalidJobStateTransitionException()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(10);
        job.Complete("result");

        // Act
        var act = () => job.MarkAsFailed("error");

        // Assert
        act.Should().Throw<InvalidJobStateTransitionException>()
            .WithMessage("*Cannot mark a completed job as failed*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkAsFailed_WithInvalidErrorMessage_ShouldThrowArgumentException(string? invalidMessage)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");

        // Act
        var act = () => job.MarkAsFailed(invalidMessage!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Error message cannot be null or empty*")
            .And.ParamName.Should().Be("errorMessage");
    }

    #endregion

    #region CanBeCancelled Tests

    [Theory]
    [InlineData(JobStatus.Queued)]
    [InlineData(JobStatus.Processing)]
    public void CanBeCancelled_InCancellableState_ShouldReturnTrue(JobStatus state)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (state == JobStatus.Processing)
        {
            job.MarkAsProcessing(10);
        }

        // Act
        var result = job.CanBeCancelled();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Cancelled)]
    [InlineData(JobStatus.Failed)]
    public void CanBeCancelled_InTerminalState_ShouldReturnFalse(JobStatus state)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (state == JobStatus.Completed)
        {
            job.MarkAsProcessing(10);
            job.Complete("result");
        }
        else if (state == JobStatus.Cancelled)
        {
            job.Cancel();
        }
        else if (state == JobStatus.Failed)
        {
            job.MarkAsFailed("error");
        }

        // Act
        var result = job.CanBeCancelled();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetProgressPercentage Tests

    [Fact]
    public void GetProgressPercentage_WithZeroTotalCharacters_ShouldReturnZero()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");

        // Act
        var percentage = job.GetProgressPercentage();

        // Assert
        percentage.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(25, 100, 25)]
    [InlineData(50, 100, 50)]
    [InlineData(75, 100, 75)]
    [InlineData(100, 100, 100)]
    [InlineData(33, 100, 33)]
    public void GetProgressPercentage_WithValidProgress_ShouldReturnCorrectPercentage(
        int processed, int total, double expected)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        job.MarkAsProcessing(total);
        job.UpdateProgress(processed);

        // Act
        var percentage = job.GetProgressPercentage();

        // Assert
        percentage.Should().BeApproximately(expected, 0.01);
    }

    #endregion

    #region IsTerminal Tests

    [Theory]
    [InlineData(JobStatus.Completed, true)]
    [InlineData(JobStatus.Cancelled, true)]
    [InlineData(JobStatus.Failed, true)]
    public void IsTerminal_InTerminalState_ShouldReturnTrue(JobStatus state, bool expected)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (state == JobStatus.Completed)
        {
            job.MarkAsProcessing(10);
            job.Complete("result");
        }
        else if (state == JobStatus.Cancelled)
        {
            job.Cancel();
        }
        else if (state == JobStatus.Failed)
        {
            job.MarkAsFailed("error");
        }

        // Act
        var result = job.IsTerminal();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(JobStatus.Queued)]
    [InlineData(JobStatus.Processing)]
    public void IsTerminal_InNonTerminalState_ShouldReturnFalse(JobStatus state)
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "test");
        
        if (state == JobStatus.Processing)
        {
            job.MarkAsProcessing(10);
        }

        // Act
        var result = job.IsTerminal();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Complete Lifecycle Tests

    [Fact]
    public void CompleteLifecycle_Success_ShouldFollowCorrectStateTransitions()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "Hello");

        // Act & Assert - Queued
        job.Status.Should().Be(JobStatus.Queued);
        job.CanBeCancelled().Should().BeTrue();
        job.IsTerminal().Should().BeFalse();

        // Act & Assert - Processing
        job.MarkAsProcessing(50);
        job.Status.Should().Be(JobStatus.Processing);
        job.CanBeCancelled().Should().BeTrue();
        job.IsTerminal().Should().BeFalse();

        // Act & Assert - Progress updates
        job.UpdateProgress(10);
        job.GetProgressPercentage().Should().Be(20);
        
        job.UpdateProgress(25);
        job.GetProgressPercentage().Should().Be(50);

        // Act & Assert - Completed
        job.Complete("result");
        job.Status.Should().Be(JobStatus.Completed);
        job.CanBeCancelled().Should().BeFalse();
        job.IsTerminal().Should().BeTrue();
        job.GetProgressPercentage().Should().Be(100);
    }

    [Fact]
    public void CompleteLifecycle_Cancelled_ShouldFollowCorrectStateTransitions()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "Hello");

        // Act & Assert - Queued
        job.Status.Should().Be(JobStatus.Queued);

        // Act & Assert - Processing
        job.MarkAsProcessing(50);
        job.Status.Should().Be(JobStatus.Processing);

        // Act & Assert - Cancelled
        job.Cancel();
        job.Status.Should().Be(JobStatus.Cancelled);
        job.CanBeCancelled().Should().BeFalse();
        job.IsTerminal().Should().BeTrue();
    }

    [Fact]
    public void CompleteLifecycle_Failed_ShouldFollowCorrectStateTransitions()
    {
        // Arrange
        var job = new JobInfo(Guid.NewGuid(), "Hello");

        // Act & Assert - Queued
        job.Status.Should().Be(JobStatus.Queued);

        // Act & Assert - Processing
        job.MarkAsProcessing(50);
        job.Status.Should().Be(JobStatus.Processing);

        // Act & Assert - Failed
        job.MarkAsFailed("Network error");
        job.Status.Should().Be(JobStatus.Failed);
        job.ErrorMessage.Should().Be("Network error");
        job.CanBeCancelled().Should().BeFalse();
        job.IsTerminal().Should().BeTrue();
    }

    #endregion
}
