using Synapse.Server.Hubs;

namespace Synapse.Server.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseWebPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }

    public static WebApplication MapWebEndpoints(this WebApplication app)
    {
        app.MapHub<GameHub>("/gameHub");
        app.MapControllers();
        return app;
    }
}