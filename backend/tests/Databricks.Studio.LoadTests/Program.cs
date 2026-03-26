using Databricks.Studio.LoadTests.Scenarios;
using NBomber.CSharp;

// ── Select which suite to run via args: "analytics" | "runs" | "all" ─────────
var suite = args.Length > 0 ? args[0].ToLower() : "all";

var scenarios = suite switch
{
    "analytics" => new[]
    {
        AnalyticsLoadScenario.BuildListScenario(),
        AnalyticsLoadScenario.BuildCreateScenario()
    },
    "runs" => new[]
    {
        AnalyticsRunLoadScenario.BuildStartStopScenario(),
        AnalyticsRunLoadScenario.BuildHistoryScenario()
    },
    _ => new[]
    {
        AnalyticsLoadScenario.BuildListScenario(),
        AnalyticsLoadScenario.BuildCreateScenario(),
        AnalyticsLoadScenario.BuildMixedScenario(),
        AnalyticsRunLoadScenario.BuildStartStopScenario(),
        AnalyticsRunLoadScenario.BuildHistoryScenario()
    }
};

NBomberRunner
    .RegisterScenarios(scenarios)
    .WithReportFolder("load-test-results")
    .WithReportFormats(NBomber.Contracts.ReportFormat.Html, NBomber.Contracts.ReportFormat.Csv)
    .Run();
