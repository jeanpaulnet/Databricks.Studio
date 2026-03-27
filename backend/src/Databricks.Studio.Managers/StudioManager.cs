using System.Text.Json;
using System.Text.Json.Serialization;
using Databricks.Studio.Entity.Data;
using Databricks.Studio.Entity.Entities;
using Databricks.Studio.Entity.Enumerations;
using Databricks.Studio.Shared.Constants;
using Databricks.Studio.Shared.DTOs;
using Databricks.Studio.Shared.DTOs.Analytics;
using Databricks.Studio.Shared.DTOs.AnalyticsRun;
using Databricks.Studio.Shared.DTOs.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Databricks.Studio.Managers;

public class StudioManager : IStudioManager
{
    private readonly StudioDbContext _db;
    private readonly ILogger<StudioManager> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public StudioManager(StudioDbContext db, ILogger<StudioManager> logger, IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
            Value = dto.Value,
            MajorVersion = 1,
            MinorVersion = 0,
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

        if (entity.Status == AnalyticsStatus.Draft)
        {
            // Draft → update in place, bump minor version
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Value = dto.Value;
            entity.MinorVersion += 1;

            await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Update, actionBy);
            await _db.SaveChangesAsync(ct);

            return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
        }
        else
        {
            // Non-draft (Submitted / Approved / Published) → create new Draft, leave original intact
            var draft = new AnalyticsEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Value = dto.Value,
                MajorVersion = entity.MajorVersion,
                MinorVersion = entity.MinorVersion + 1,
                Status = AnalyticsStatus.Draft,
                // Link back to the root of this analytics family
                OriginalId = entity.OriginalId ?? entity.Id
            };

            _db.Analytics.Add(draft);
            await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, draft, AppConstants.ActionTypes.Create, actionBy);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Created new draft v{Major}.{Minor} from analytics {SourceId}",
                draft.MajorVersion, draft.MinorVersion, id);

            return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(draft));
        }
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
            .Select(x => new AnalyticsListDto(x.Id, x.Name, x.Description, x.Value, x.MajorVersion, x.MinorVersion, (int)x.Status, x.Status.ToString(), x.OriginalId))
            .ToListAsync(ct);

        return ApiResponse<PagedResult<AnalyticsListDto>>.Ok(new PagedResult<AnalyticsListDto>(items, total, page, pageSize));
    }

    public async Task<ApiResponse<AnalyticsSummaryDto>> GetAnalyticsSummaryAsync(CancellationToken ct = default)
    {
        var groups = await _db.Analytics
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(x => x.Value) })
            .ToListAsync(ct);

        var byStatus = groups.Select(g => new StatusCountDto(g.Status.ToString(), g.Count, g.Total));
        var total = groups.Sum(g => g.Count);
        var totalValue = groups.Sum(g => g.Total);
        var avg = total > 0 ? totalValue / total : 0;

        return ApiResponse<AnalyticsSummaryDto>.Ok(new AnalyticsSummaryDto(total, totalValue, avg, byStatus));
    }

    public async Task<ApiResponse<AnalyticsDto>> SubmitAnalyticsAsync(Guid id, string actionBy, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.FindAsync([id], ct);
        if (entity is null) return ApiResponse<AnalyticsDto>.Fail($"Analytics {id} not found.");
        if (entity.Status != AnalyticsStatus.Draft)
            return ApiResponse<AnalyticsDto>.Fail("Only Draft analytics can be submitted for review.");

        entity.Status = AnalyticsStatus.Submitted;
        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Update, actionBy);
        await _db.SaveChangesAsync(ct);
        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
    }

    public async Task<ApiResponse<AnalyticsDto>> PublishAnalyticsAsync(Guid id, string actionBy, CancellationToken ct = default)
    {
        var entity = await _db.Analytics.FindAsync([id], ct);
        if (entity is null) return ApiResponse<AnalyticsDto>.Fail($"Analytics {id} not found.");
        if (entity.Status != AnalyticsStatus.Approved)
            return ApiResponse<AnalyticsDto>.Fail("Only Approved analytics can be published.");

        entity.Status = AnalyticsStatus.Published;
        entity.MajorVersion += 1;   // bump major version on publish
        entity.MinorVersion = 0;
        await RecordHistoryAsync(AppConstants.EntityTypes.Analytics, entity, AppConstants.ActionTypes.Update, actionBy);
        await _db.SaveChangesAsync(ct);
        return ApiResponse<AnalyticsDto>.Ok(MapAnalytics(entity));
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
            InputJson = dto.InputJson,
            OutputJson = dto.OutputJson,
            MajorVersion = dto.MajorVersion > 0 ? dto.MajorVersion : analytics.MajorVersion,
            Status = AnalyticsRunStatus.Started,
            StartedOn = DateTime.UtcNow
        };

        _db.AnalyticsRuns.Add(run);
        await RecordHistoryAsync(AppConstants.EntityTypes.AnalyticsRun, run, AppConstants.ActionTypes.Create, dto.StartedBy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Started analytics run {RunId} for analytics {AnalyticsId}", run.Id, analyticsId);

        // Mock: auto-complete after 5 minutes
        var runId = run.Id;
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StudioDbContext>();
            var bgRun = await db.AnalyticsRuns.FindAsync(runId);
            if (bgRun is { Status: AnalyticsRunStatus.Started })
            {
                bgRun.Status = AnalyticsRunStatus.Completed;
                bgRun.CompletedOn = DateTime.UtcNow;
                await db.SaveChangesAsync();
                _logger.LogInformation("Mock completed analytics run {RunId}", runId);
            }
        });

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

    private static readonly JsonSerializerOptions _historyJsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private Task RecordHistoryAsync<T>(string entityType, T entity, string actionType, string actionBy)
    {
        _db.History.Add(new HistoryEntity
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityJson = JsonSerializer.Serialize(entity, _historyJsonOptions),
            ActionType = actionType,
            ActionBy = actionBy,
            ActionOn = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    private static AnalyticsDto MapAnalytics(AnalyticsEntity e) =>
        new(e.Id, e.Name, e.Description, e.Value, e.MajorVersion, e.MinorVersion, (int)e.Status, e.Status.ToString(), e.OriginalId);

    private static AnalyticsRunDto MapRun(AnalyticsRunEntity r) =>
        new(r.Id, r.AnalyticsId, r.JobId, (int)r.Status, r.Status.ToString(),
            r.StartedOn, r.CompletedOn, r.TerminatedOn, r.InputJson, r.OutputJson, r.MajorVersion);
}
