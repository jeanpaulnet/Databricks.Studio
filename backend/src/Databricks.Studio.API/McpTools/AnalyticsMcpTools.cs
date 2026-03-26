using System.ComponentModel;
using System.Text.Json;
using Databricks.Studio.Managers;
using ModelContextProtocol.Server;

namespace Databricks.Studio.API.McpTools;

[McpServerToolType]
public sealed class AnalyticsMcpTools(IStudioManager studio)
{
    [McpServerTool(Name = "get_analytics_summary")]
    [Description("Returns count of analytics items by status, total value, and average value")]
    public async Task<string> GetAnalyticsSummaryAsync()
    {
        var result = await studio.GetAnalyticsSummaryAsync();
        return JsonSerializer.Serialize(result.Data);
    }

    [McpServerTool(Name = "list_analytics")]
    [Description("Lists all analytics items with name, status, and value")]
    public async Task<string> ListAnalyticsAsync()
    {
        var result = await studio.ListAnalyticsAsync(1, 100);
        return JsonSerializer.Serialize(result.Data?.Items);
    }
}
