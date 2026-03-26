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
    int Status,
    string StatusName
);

public record ReviewAnalyticsDto(
    string ReviewedBy,
    string? Comments
);
