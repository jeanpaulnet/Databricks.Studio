namespace Databricks.Studio.Shared.DTOs.AnalyticsRun;

public record StartAnalyticsRunDto(
    string JobId,
    string StartedBy,
    string? InputJson,
    string? OutputJson,
    int MajorVersion = 1
);

public record StopAnalyticsRunDto(
    string StoppedBy
);

public record AnalyticsRunDto(
    Guid Id,
    Guid AnalyticsId,
    string JobId,
    int Status,
    string StatusName,
    DateTime StartedOn,
    DateTime? CompletedOn,
    DateTime? TerminatedOn,
    string? InputJson,
    string? OutputJson,
    int MajorVersion
);
