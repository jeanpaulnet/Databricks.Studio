namespace Databricks.Studio.LoadTests.Config;

public static class LoadTestConfig
{
    public const string BaseUrl = "https://localhost:7001";
    public const string BearerToken = "YOUR_TEST_JWT_TOKEN"; // Replace with a valid dev token

    // Scenario durations
    public static readonly TimeSpan WarmupDuration = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan RunDuration = TimeSpan.FromSeconds(60);

    // Concurrency
    public const int LightLoad = 10;
    public const int MediumLoad = 50;
    public const int HeavyLoad = 150;
}
