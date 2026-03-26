using Databricks.Studio.LoadTests.Config;
using NBomber.CSharp;

namespace Databricks.Studio.LoadTests.Scenarios;

public static class AnalyticsRunLoadScenario
{
    private static readonly Guid TestAnalyticsId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static ScenarioProps BuildStartStopScenario()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(LoadTestConfig.BaseUrl) };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {LoadTestConfig.BearerToken}");

        var startStep = Step.Create("POST run/start/{analyticsId}", async context =>
        {
            var payload = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    jobId = $"load-job-{Guid.NewGuid()}",
                    startedBy = "load-test-user"
                }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync($"/api/analytics/run/start/{TestAnalyticsId}", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return Response.Fail(statusCode: (int)response.StatusCode, error: content);

            using var doc = System.Text.Json.JsonDocument.Parse(content);
            var runId = doc.RootElement.GetProperty("data").GetProperty("id").GetString();
            return Response.Ok(payload: runId, statusCode: (int)response.StatusCode);
        });

        var stopStep = Step.Create("POST run/stop/{runId}", async context =>
        {
            var runId = context.PreviousResponse.Payload.Value as string;
            if (string.IsNullOrEmpty(runId)) return Response.Ok();

            var payload = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new { stoppedBy = "load-test-user" }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync($"/api/analytics/run/stop/{runId}", payload);
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
        });

        return ScenarioBuilder
            .CreateScenario("analytics_run_start_stop", startStep, stopStep)
            .WithWarmUpDuration(LoadTestConfig.WarmupDuration)
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: LoadTestConfig.LightLoad, during: LoadTestConfig.RunDuration)
            );
    }

    public static ScenarioProps BuildHistoryScenario()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(LoadTestConfig.BaseUrl) };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {LoadTestConfig.BearerToken}");

        var step = Step.Create("GET run/history/{analyticsId}", async context =>
        {
            var response = await httpClient.GetAsync($"/api/analytics/run/history/{TestAnalyticsId}");
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
        });

        return ScenarioBuilder
            .CreateScenario("analytics_run_history", step)
            .WithWarmUpDuration(LoadTestConfig.WarmupDuration)
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: LoadTestConfig.MediumLoad, during: LoadTestConfig.RunDuration)
            );
    }
}
