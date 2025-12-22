namespace Synapse.Server.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<WorldSimulation>();
        // services.AddHostedService<BotSimulationService>();
        return services;
    }

    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
            options.EnableDetailedErrors = true;
        });
        services.AddControllers();
        
        return services;
    }
}