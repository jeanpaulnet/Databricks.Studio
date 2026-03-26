using Databricks.Studio.LoadTests.Config;
using NBomber.CSharp;

namespace Databricks.Studio.LoadTests.Scenarios;

public static class AnalyticsLoadScenario
{
    public static ScenarioProps BuildListScenario()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(LoadTestConfig.BaseUrl) };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {LoadTestConfig.BearerToken}");

        var step = Step.Create("GET manage/list", async context =>
        {
            var response = await httpClient.GetAsync("/api/analytics/manage/list?page=1&pageSize=20");
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode, error: response.ReasonPhrase);
        });

        return ScenarioBuilder
            .CreateScenario("analytics_list", step)
            .WithWarmUpDuration(LoadTestConfig.WarmupDuration)
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: LoadTestConfig.LightLoad, during: LoadTestConfig.RunDuration)
            );
    }

    public static ScenarioProps BuildCreateScenario()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(LoadTestConfig.BaseUrl) };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {LoadTestConfig.BearerToken}");

        var step = Step.Create("POST manage/create", async context =>
        {
            var payload = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    name = $"LoadTest_{Guid.NewGuid()}",
                    description = "Created by load test"
                }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync("/api/analytics/manage/create", payload);
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode, error: response.ReasonPhrase);
        });

        return ScenarioBuilder
            .CreateScenario("analytics_create", step)
            .WithWarmUpDuration(LoadTestConfig.WarmupDuration)
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: LoadTestConfig.LightLoad, during: LoadTestConfig.RunDuration)
            );
    }

    public static ScenarioProps BuildMixedScenario()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(LoadTestConfig.BaseUrl) };
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {LoadTestConfig.BearerToken}");

        var listStep = Step.Create("GET manage/list", async context =>
        {
            var response = await httpClient.GetAsync("/api/analytics/manage/list?page=1&pageSize=20");
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
        });

        var createStep = Step.Create("POST manage/create", async context =>
        {
            if (context.InvocationNumber % 5 != 0)
                return Response.Ok();

            var payload = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    name = $"Mixed_{Guid.NewGuid()}",
                    description = "Mixed load test"
                }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync("/api/analytics/manage/create", payload);
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: (int)response.StatusCode)
                : Response.Fail(statusCode: (int)response.StatusCode);
        });

        return ScenarioBuilder
            .CreateScenario("analytics_mixed", listStep, createStep)
            .WithWarmUpDuration(LoadTestConfig.WarmupDuration)
            .WithLoadSimulations(
                Simulation.RampingInject(rate: LoadTestConfig.MediumLoad, interval: TimeSpan.FromSeconds(10), during: LoadTestConfig.RunDuration)
            );
    }
}
