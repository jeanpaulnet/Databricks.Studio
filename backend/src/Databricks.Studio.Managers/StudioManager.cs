using System.Text.Json;
using Databricks.Studio.Entity.Data;
using Databricks.Studio.Entity.Entities;
using Databricks.Studio.Entity.Enumerations;
using Databricks.Studio.Shared.Constants;
using Databricks.Studio.Shared.DTOs;
using Databricks.Studio.Shared.DTOs.Analytics;
using Databricks.Studio.Shared.DTOs.AnalyticsRun;
using Databricks.Studio.Shared.DTOs.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Databricks.Studio.Managers;

public class StudioManager : IStudioManager
{
    private readonly StudioDbContext _db;
    private readonly ILogger<StudioManager> _logger;

    public StudioManager(StudioDbContext db, ILogger<StudioManager> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Analytics ─────────────────────────────────────────────────────────────

    public async Task<ApiResponse<AnalyticsDto>> CreateAnalyticsAsync(CreateAnalyticsDto dto, string actionBy, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating analytics: {Name}", dto.Name);

        var entity = new AnalyticsEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Status = AnalyticsStatus.Draft
        };

        _db.Analytics.Add(entity);
        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Create, actionBy);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
    }

    public async Task<ApiResponse<AnalyticsDto>> UpdateAnalyticsAsync(Guid id, UpdateAnalyticsDto dto, string actionBy, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.FindAsync([id], ct);
        if (entity is null)
            return ApiResponse<AnalyticsDto>.Fail($"Analytics {id} not found.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Update, actionBy);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
    }

    public async Task<ApiResponse<bool>> DeleteAnalyticsAsync(Guid id, string actionBy, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.FindAsync([id], ct);
        if (entity is null)
            return ApiResponse<bool>.Fail($"Analytics {id} not found.");

        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Delete, actionBy);
        _db.Analytics.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<AnalyticsDto>> GetAnalyticsByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return ApiResponse<AnalyticsDto>.Fail($"Analytics {id} not found.");

        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
    }

    public async Task<ApiResponse<PagedResult<AnalyticsListDto>>> ListAnalyticsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Analytics.AsNoTracking();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AnalyticsListDto(x.Id, x.Name, (int)x.Status, x.Status.ToString()))
            .ToListAsync(ct);

        return ApiResponse<PagedResult<AnalyticsListDto>>.Ok(new PagedResult<AnalyticsListDto>(items, total, page, pageSize));
    }

    public async Task<ApiResponse<AnalyticsDto>> ApproveAnalyticsAsync(Guid id, ReviewAnalyticsDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.FindAsync([id], ct);
        if (entity is null)
            return ApiResponse<AnalyticsDto>.Fail($"Analytics {id} not found.");

        if (entity.Status != AnalyticsStatus.Submitted)
            return ApiResponse<AnalyticsDto>.Fail("Only Submitted analytics can be approved.");

        entity.Status = AnalyticsStatus.Approved;
        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Update, dto.ReviewedBy);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
    }

    public async Task<ApiResponse<AnalyticsDto>> RejectAnalyticsAsync(Guid id, ReviewAnalyticsDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.FindAsync([id], ct);
        if (entity is null)
            return ApiResponse<AnalyticsDto>.Fail($"Analytics {id} not found.");

        if (entity.Status != AnalyticsStatus.Submitted)
            return ApiResponse<AnalyticsDto>.Fail("Only Submitted analytics can be rejected.");

        entity.Status = AnalyticsStatus.Rejected;
        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Update, dto.ReviewedBy);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
    }

    // ── Analytics Runs ────────────────────────────────────────────────────────

    public async Task<ApiResponse<AnalyticsRunDto>> StartRunAsync(Guid analyticsId, StartAnalyticsRunDto dto, CancellationToken ct = default)
    {
        var analytics = await _db.Analytics.FindAsync([analyticsId], ct);
        if (analytics is null)
            return ApiResponse<AnalyticsRunDto>.Fail($"Analytics {analyticsId} not found.");

        if (analytics.Status != AnalyticsStatus.Published)
            return ApiResponse<AnalyticsRunDto>.Fail("Only Published analytics can be run.");

        var run = new AnalyticsRunEntity
        {
            Id = Guid.NewGuid(),
            AnalyticsId = analyticsId,
            JobId = dto.JobId,
            Status = AnalyticsRunStatus.Queued,
            StartedOn = DateTime.UtcNow
        };

        _db.AnalyticsRuns.Add(run);
        await RecordHistoryAsync(AppConstants.EntityTypes.AnalyticsRun, run, AppConstants.ActionTypes.Create, dto.StartedBy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Started analytics run {RunId} for analytics {AnalyticsId}", run.Id, analyticsId);
        return ApiResponse<AnalyticsRunDto>.Ok(MapRun(run));
    }

    public async Task<ApiResponse<AnalyticsRunDto>> StopRunAsync(Guid runId, StopAnalyticsRunDto dto, CancellationToken ct = default)
    {
        var run = await _db.AnalyticsRuns.FindAsync([runId], ct);
        if (run is null)
            return ApiResponse<AnalyticsRunDto>.Fail($"Run {runId} not found.");

        if (run.Status is AnalyticsRunStatus.Completed or AnalyticsRunStatus.Terminated)
            return ApiResponse<AnalyticsRunDto>.Fail("Run has already finished.");

        run.Status = AnalyticsRunStatus.Terminated;
        run.TerminatedOn = DateTime.UtcNow;

        await RecordHistoryAsync(AppConstants.EntityTypes.AnalyticsRun, run, AppConstants.ActionTypes.Update, dto.StoppedBy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Stopped analytics run {RunId}", runId);
        return ApiResponse<AnalyticsRunDto>.Ok(MapRun(run));
    }

    public async Task<ApiResponse<AnalyticsRunDto>> GetRunByIdAsync(Guid runId, CancellationToken ct = default)
    {
        var run = await _db.AnalyticsRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId, ct);
        if (run is null)
            return ApiResponse<AnalyticsRunDto>.Fail($"Run {runId} not found.");

        return ApiResponse<AnalyticsRunDto>.Ok(MapRun(run));
    }

    public async Task<ApiResponse<IEnumerable<HistoryDto>>> GetRunHistoryAsync(Guid analyticsId, CancellationToken ct = default)
    {
        var history = await _db.History
            .AsNoTracking()
            .Where(h => h.EntityType == AppConstants.EntityTypes.AnalyticsRun)
            .OrderByDescending(h => h.ActionOn)
            .ToListAsync(ct);

        var filtered = history
            .Where(h => h.EntityJson.Contains(analyticsId.ToString()))
            .Select(h => new HistoryDto(h.Id, h.EntityType, h.EntityJson, h.ActionType, h.ActionBy, h.ActionOn));

        return ApiResponse<IEnumerable<HistoryDto>>.Ok(filtered);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private Task RecordHistoryAsync<T>(string entityType, T entity, string actionType, string actionBy)
    {
        _db.History.Add(new HistoryEntity
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityJson = JsonSerializer.Serialize(entity),
            ActionType = actionType,
            ActionBy = actionBy,
            ActionOn = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    private static AnalyticsDto MapAnalytics(AnalyticsEntity e) =>
        new(e.Id, e.Name, e.Description, (int)e.Status, e.Status.ToString());

    private static AnalyticsRunDto MapRun(AnalyticsRunEntity r) =>
        new(r.Id, r.AnalyticsId, r.JobId, (int)r.Status, r.Status.ToString(),
            r.StartedOn, r.CompletedOn, r.TerminatedOn);
}
