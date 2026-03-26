using Databricks.Studio.Entity.Enumerations;

namespace Databricks.Studio.Entity.Entities;

public class AnalyticsEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AnalyticsStatus Status { get; set; } = AnalyticsStatus.Draft;
    public double Value { get; set; } = 0;

    public ICollection<AnalyticsRunEntity> Runs { get; set; } = new List<AnalyticsRunEntity>();
}
