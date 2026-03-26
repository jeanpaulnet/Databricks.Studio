using Databricks.Studio.Managers;
using Databricks.Studio.Shared.Constants;
using Databricks.Studio.Shared.DTOs.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace Databricks.Studio.API.Controllers;

[ApiController]
[Route("api/analytics/review")]
public class AnalyticsReviewController : ControllerBase
{
    private readonly IStudioManager _manager;
    private readonly ILogger<AnalyticsReviewController> _logger;

    public AnalyticsReviewController(IStudioManager manager, ILogger<AnalyticsReviewController> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    // POST api/analytics/review/approve/{id}
    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewAnalyticsDto dto, CancellationToken ct = default)
    {
        var result = await _manager.ApproveAnalyticsAsync(id, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST api/analytics/review/reject/{id}
    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewAnalyticsDto dto, CancellationToken ct = default)
    {
        var result = await _manager.RejectAnalyticsAsync(id, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
