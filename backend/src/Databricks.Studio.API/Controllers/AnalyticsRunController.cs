using Databricks.Studio.Managers;
using Databricks.Studio.Shared.Constants;
using Databricks.Studio.Shared.DTOs.AnalyticsRun;
using Microsoft.AspNetCore.Mvc;

namespace Databricks.Studio.API.Controllers;

[ApiController]
[Route("api/analytics/run")]
public class AnalyticsRunController : ControllerBase
{
    private readonly IStudioManager _manager;
    private readonly ILogger<AnalyticsRunController> _logger;

    public AnalyticsRunController(IStudioManager manager, ILogger<AnalyticsRunController> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    // POST api/analytics/run/start/{analyticsId}
    [HttpPost("start/{analyticsId:guid}")]
    public async Task<IActionResult> Start(Guid analyticsId, [FromBody] StartAnalyticsRunDto dto, CancellationToken ct = default)
    {
        var result = await _manager.StartRunAsync(analyticsId, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST api/analytics/run/stop/{runId}
    [HttpPost("stop/{runId:guid}")]
    public async Task<IActionResult> Stop(Guid runId, [FromBody] StopAnalyticsRunDto dto, CancellationToken ct = default)
    {
        var result = await _manager.StopRunAsync(runId, dto, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET api/analytics/run/get/{runId}
    [HttpGet("get/{runId:guid}")]
    public async Task<IActionResult> Get(Guid runId, CancellationToken ct = default)
    {
        var result = await _manager.GetRunByIdAsync(runId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // GET api/analytics/run/history/{analyticsId}
    [HttpGet("history/{analyticsId:guid}")]
    public async Task<IActionResult> History(Guid analyticsId, CancellationToken ct = default)
    {
        var result = await _manager.GetRunHistoryAsync(analyticsId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
