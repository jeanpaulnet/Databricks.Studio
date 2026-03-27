using Databricks.Studio.Entity.Enumerations;

namespace Databricks.Studio.Entity.Entities;

public class AnalyticsEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AnalyticsStatus Status { get; set; } = AnalyticsStatus.Draft;
    public double Value { get; set; } = 0;
    public int MajorVersion { get; set; } = 1;
    public int MinorVersion { get; set; } = 0;

    /// <summary>
    /// Points to the first-ever version of this analytics family.
    /// Null on the original; set on every subsequent draft.
    /// </summary>
    public Guid? OriginalId { get; set; }

    public ICollection<AnalyticsRunEntity> Runs { get; set; } = new List<AnalyticsRunEntity>();
}
