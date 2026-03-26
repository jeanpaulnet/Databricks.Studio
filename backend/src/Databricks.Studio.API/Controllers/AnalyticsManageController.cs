using Databricks.Studio.Managers;
using Databricks.Studio.Shared.Constants;
using Databricks.Studio.Shared.DTOs.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace Databricks.Studio.API.Controllers;

[ApiController]
[Route("api/analytics/manage")]
public class AnalyticsManageController : ControllerBase
{
    private readonly IStudioManager _manager;
    private readonly ILogger<AnalyticsManageController> _logger;

    public AnalyticsManageController(IStudioManager manager, ILogger<AnalyticsManageController> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    // GET api/analytics/manage/list
    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _manager.ListAnalyticsAsync(page, pageSize, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET api/analytics/manage/get/{id}
    [HttpGet("get/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await _manager.GetAnalyticsByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST api/analytics/manage/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateAnalyticsDto dto, CancellationToken ct = default)
    {
        var actionBy = User.Identity?.Name ?? "anonymous";
        var result = await _manager.CreateAnalyticsAsync(dto, actionBy, ct);
        return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    // PUT api/analytics/manage/update/{id}
    [HttpPut("update/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnalyticsDto dto, CancellationToken ct = default)
    {
        var actionBy = User.Identity?.Name ?? "anonymous";
        var result = await _manager.UpdateAnalyticsAsync(id, dto, actionBy, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE api/analytics/manage/delete/{id}
    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var actionBy = User.Identity?.Name ?? "anonymous";
        var result = await _manager.DeleteAnalyticsAsync(id, actionBy, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
