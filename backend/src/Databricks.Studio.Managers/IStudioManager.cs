using Databricks.Studio.Shared.DTOs;
using Databricks.Studio.Shared.DTOs.Analytics;
using Databricks.Studio.Shared.DTOs.AnalyticsRun;
using Databricks.Studio.Shared.DTOs.History;

namespace Databricks.Studio.Managers;

public interface IStudioManager
{
    // ── Analytics ─────────────────────────────────────────────────────────────
    Task<ApiResponse<AnalyticsDto>> CreateAnalyticsAsync(CreateAnalyticsDto dto, string actionBy, CancellationToken ct = default);
    Task<ApiResponse<AnalyticsDto>> UpdateAnalyticsAsync(Guid id, UpdateAnalyticsDto dto, string actionBy, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAnalyticsAsync(Guid id, string actionBy, CancellationToken ct = default);
    Task<ApiResponse<AnalyticsDto>> GetAnalyticsByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<AnalyticsListDto>>> ListAnalyticsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ApiResponse<AnalyticsSummaryDto>> GetAnalyticsSummaryAsync(CancellationToken ct = default);
    Task<ApiResponse<AnalyticsDto>> ApproveAnalyticsAsync(Guid id, ReviewAnalyticsDto dto, CancellationToken ct = default);
    Task<ApiResponse<AnalyticsDto>> RejectAnalyticsAsync(Guid id, ReviewAnalyticsDto dto, CancellationToken ct = default);

    // ── Analytics Runs ────────────────────────────────────────────────────────
    Task<ApiResponse<AnalyticsRunDto>> StartRunAsync(Guid analyticsId, StartAnalyticsRunDto dto, CancellationToken ct = default);
    Task<ApiResponse<AnalyticsRunDto>> StopRunAsync(Guid runId, StopAnalyticsRunDto dto, CancellationToken ct = default);
    Task<ApiResponse<AnalyticsRunDto>> GetRunByIdAsync(Guid runId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<HistoryDto>>> GetRunHistoryAsync(Guid analyticsId, CancellationToken ct = default);
}
