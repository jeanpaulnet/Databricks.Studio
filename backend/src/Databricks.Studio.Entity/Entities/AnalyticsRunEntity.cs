using Databricks.Studio.Entity.Enumerations;

namespace Databricks.Studio.Entity.Entities;

public class AnalyticsRunEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AnalyticsId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public AnalyticsRunStatus Status { get; set; } = AnalyticsRunStatus.Queued;
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public int MajorVersion { get; set; } = 1;
    public DateTime StartedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public DateTime? TerminatedOn { get; set; }

    public AnalyticsEntity Analytics { get; set; } = null!;
}
