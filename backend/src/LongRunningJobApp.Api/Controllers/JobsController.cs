using LongRunningJobApp.Application.DTOs;
using LongRunningJobApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LongRunningJobApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobService jobService,
        ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new job to process the input string
    /// Returns jobId immediately, client should connect to SignalR for real-time updates
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateJobResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateJobResponse>> CreateJob(
        [FromBody] ProcessJobRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest(new { error = "Input cannot be empty" });
        }

        try
        {
            var job = await _jobService.CreateJobAsync(request.Input, cancellationToken);
            
            var hubUrl = $"{Request.Scheme}://{Request.Host}/hub/job-progress";

            var response = new CreateJobResponse
            {
                JobId = job.Id,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                HubUrl = hubUrl
            };

            _logger.LogInformation("Job {JobId} created for processing", job.Id);

            return Accepted(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create job");
            return StatusCode(500, new { error = "Failed to create job" });
        }
    }

    /// <summary>
    /// Attempts to cancel a job
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(CancelJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancelJobResponse>> CancelJob(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = _jobService.GetJob(id);

        if (job == null)
        {
            return NotFound(new { error = $"Job {id} not found" });
        }

        var cancelled = await _jobService.CancelJobAsync(id, cancellationToken);

        var response = new CancelJobResponse
        {
            Success = cancelled,
            Message = cancelled
                ? "Job cancelled successfully"
                : $"Job cannot be cancelled. Current status: {job.Status}"
        };

        _logger.LogInformation("Cancel requested for job {JobId}. Result: {Success}", id, cancelled);

        return Ok(response);
    }
}
