using RedAlertLEDs.Services;
using RedAlertLEDs.Services.HomeFrontCommand;
using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.Polygons;
using RedAlertLEDs.Services.Predictor;
using RedAlertLEDs.Services.StateManager;
using Serilog;

namespace RedAlertLEDs.Extensions;

public static class AppBuilderExtensions
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<PolygonsService>();
        builder.Services.AddSingleton<PredictorService>();
        builder.Services.AddSingleton<StateManagerService>();
        builder.Services.AddSingleton<LedStripService>();
    }

    public static void AddBackgroundServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<HomeFrontCommandPoller>();

        builder.Services.AddHostedService(provider =>
            provider.GetRequiredService<HomeFrontCommandPoller>());
    }

    public static void AddOrchestrators(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<Orchestrator>();
        builder.Services.AddHostedService<StartupInitializer>();
    }

    public static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });
    }
}