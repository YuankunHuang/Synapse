namespace Synapse.Server.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<WorldSimulation>();
        // services.AddHostedService<BotSimulationService>();
        return services;
    }
}