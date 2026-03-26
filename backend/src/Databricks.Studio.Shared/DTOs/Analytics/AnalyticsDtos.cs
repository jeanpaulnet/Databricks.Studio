namespace Databricks.Studio.Shared.DTOs.Analytics;

public record CreateAnalyticsDto(
    string Name,
    string Description
);

public record UpdateAnalyticsDto(
    string Name,
    string Description
);

public record AnalyticsDto(
    Guid Id,
    string Name,
    string Description,
    int Status,
    string StatusName
);

public record AnalyticsListDto(
    Guid Id,
    string Name,
    string? Description,
    double Value,
    int Status,
    string StatusName
);

public record ReviewAnalyticsDto(
    string ReviewedBy,
    string? Comments
);

public record StatusCountDto(
    string Status,
    int Count,
    double TotalValue
);

public record AnalyticsSummaryDto(
    int TotalCount,
    double TotalValue,
    double AverageValue,
    IEnumerable<StatusCountDto> CountByStatus
);
