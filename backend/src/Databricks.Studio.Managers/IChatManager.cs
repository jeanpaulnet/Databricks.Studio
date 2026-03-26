using Databricks.Studio.Shared.DTOs.Chat;

namespace Databricks.Studio.Managers;

public interface IChatManager
{
    Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken ct = default);
}
