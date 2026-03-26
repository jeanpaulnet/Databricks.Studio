using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Databricks.Studio.Shared.DTOs.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Databricks.Studio.Managers;

public class ChatManager(HttpClient httpClient, IStudioManager studio, IOptions<AnthropicOptions> options, ILogger<ChatManager> logger) : IChatManager
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string SystemPrompt =
        "You are a friendly data assistant for Databricks Studio. " +
        "Use the available tools to look up analytics data, then answer in plain conversational English. " +
        "Never return raw JSON or technical data structures. " +
        "Present numbers clearly (e.g. '5 analytics are currently published'). " +
        "Use bullet points or short sentences to summarise multiple values. " +
        "Keep answers concise and easy to read.";

    public async Task<ChatResponseDto> ChatAsync(ChatRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey) || options.Value.ApiKey == "YOUR_ANTHROPIC_API_KEY")
            return new ChatResponseDto("Anthropic API key is not configured. Please set Anthropic:ApiKey in appsettings.");

        var messages = new List<object>();

        foreach (var h in request.History ?? [])
            messages.Add(new { role = h.Role, content = h.Content });

        messages.Add(new { role = "user", content = request.Message });

        for (var iteration = 0; iteration < 5; iteration++)
        {
            var body = JsonSerializer.Serialize(new
            {
                model = options.Value.Model,
                max_tokens = 1024,
                system = SystemPrompt,
                tools = GetToolDefinitions(),
                messages
            });

            var req = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("x-api-key", options.Value.ApiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");

            var res = await httpClient.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
            {
                var errorBody = await res.Content.ReadAsStringAsync(ct);
                logger.LogError("Anthropic API error {Status}: {Body}", (int)res.StatusCode, errorBody);
                return new ChatResponseDto($"AI service error ({(int)res.StatusCode}). Check the API key and model configuration.");
            }

            var json = await res.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var stopReason = root.GetProperty("stop_reason").GetString();
            var content = root.GetProperty("content");

            if (stopReason == "end_turn")
            {
                foreach (var block in content.EnumerateArray())
                    if (block.GetProperty("type").GetString() == "text")
                        return new ChatResponseDto(block.GetProperty("text").GetString() ?? "");
                break;
            }

            if (stopReason == "tool_use")
            {
                var contentClone = content.Clone();
                messages.Add(new { role = "assistant", content = contentClone });

                var toolResults = new List<object>();
                foreach (var block in content.EnumerateArray())
                {
                    if (block.GetProperty("type").GetString() != "tool_use") continue;

                    var toolName = block.GetProperty("name").GetString() ?? "";
                    var toolId = block.GetProperty("id").GetString() ?? "";
                    var result = await ExecuteToolAsync(toolName, ct);

                    toolResults.Add(new { type = "tool_result", tool_use_id = toolId, content = result });
                }

                messages.Add(new { role = "user", content = toolResults });
            }
        }

        return new ChatResponseDto("I was unable to complete the request.");
    }

    private async Task<string> ExecuteToolAsync(string toolName, CancellationToken ct) => toolName switch
    {
        "get_analytics_summary" => JsonSerializer.Serialize((await studio.GetAnalyticsSummaryAsync(ct)).Data),
        "list_analytics" => JsonSerializer.Serialize((await studio.ListAnalyticsAsync(1, 100, ct)).Data?.Items),
        _ => $"Unknown tool: {toolName}"
    };

    private static object[] GetToolDefinitions() =>
    [
        new
        {
            name = "get_analytics_summary",
            description = "Returns count of analytics items grouped by status, total value of all analytics, and average value",
            input_schema = new { type = "object", properties = new { }, required = Array.Empty<string>() }
        },
        new
        {
            name = "list_analytics",
            description = "Lists all analytics items with their name, status code, status name, and value",
            input_schema = new { type = "object", properties = new { }, required = Array.Empty<string>() }
        }
    ];
}
