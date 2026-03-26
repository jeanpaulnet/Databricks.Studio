using Microsoft.Extensions.DependencyInjection;

namespace Databricks.Studio.Managers;

public static class ManagerServiceExtensions
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services.AddScoped<IStudioManager, StudioManager>();
        services.AddHttpClient<IChatManager, ChatManager>();
        return services;
    }
}
