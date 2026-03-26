namespace Databricks.Studio.Shared.DTOs.AnalyticsRun;

public record StartAnalyticsRunDto(
    string JobId,
    string StartedBy
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
    DateTime? TerminatedOn
);
