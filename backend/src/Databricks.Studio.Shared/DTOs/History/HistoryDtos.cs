namespace Databricks.Studio.Shared.DTOs.History;

public record HistoryDto(
    Guid Id,
    string EntityType,
    string EntityJson,
    string ActionType,
    string ActionBy,
    DateTime ActionOn
);
