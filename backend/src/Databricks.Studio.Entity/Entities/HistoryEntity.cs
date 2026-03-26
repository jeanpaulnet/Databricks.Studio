namespace Databricks.Studio.Entity.Entities;

public class HistoryEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityType { get; set; } = string.Empty;
    public string EntityJson { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;  // create | update | delete
    public string ActionBy { get; set; } = string.Empty;
    public DateTime ActionOn { get; set; } = DateTime.UtcNow;
}
