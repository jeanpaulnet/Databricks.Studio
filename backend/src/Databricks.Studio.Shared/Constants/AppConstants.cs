namespace Databricks.Studio.Shared.Constants;

public static class AppConstants
{
    public static class ActionTypes
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
    }

    public static class EntityTypes
    {
        public const string Analytics = nameof(Analytics);
        public const string AnalyticsRun = nameof(AnalyticsRun);
    }

    public static class Policies
    {
        public const string Authenticated = "Authenticated";
        public const string Reviewer = "Reviewer";
    }

    public static class CorsPolicies
    {
        public const string AllowAngularDev = "AllowAngularDev";
    }
}
