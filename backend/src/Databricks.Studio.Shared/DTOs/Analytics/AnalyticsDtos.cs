namespace Databricks.Studio.Shared.DTOs.Analytics;

public record CreateAnalyticsDto(
    string Name,
    string Description,
    double Value = 0
);

public record UpdateAnalyticsDto(
    string Name,
    string Description,
    double Value = 0
);

public record AnalyticsDto(
    Guid Id,
    string Name,
    string Description,
    double Value,
    int MajorVersion,
    int MinorVersion,
    int Status,
    string StatusName,
    Guid? OriginalId
);

public record AnalyticsListDto(
    Guid Id,
    string Name,
    string? Description,
    double Value,
    int MajorVersion,
    int MinorVersion,
    int Status,
    string StatusName,
    Guid? OriginalId
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
