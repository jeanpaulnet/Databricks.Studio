namespace Databricks.Studio.Shared.DTOs.Chat;

public record ChatMessageDto(string Role, string Content);
public record ChatRequestDto(string Message, IEnumerable<ChatMessageDto>? History);
public record ChatResponseDto(string Reply);
