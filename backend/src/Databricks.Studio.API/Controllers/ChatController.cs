using Databricks.Studio.Managers;
using Databricks.Studio.Shared.DTOs.Chat;
using Microsoft.AspNetCore.Mvc;

namespace Databricks.Studio.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IChatManager chat) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request, CancellationToken ct)
    {
        var response = await chat.ChatAsync(request, ct);
        return Ok(response);
    }
}
