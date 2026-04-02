using RedAlertLEDs.Repositories;
using RedAlertLEDs.Services;
using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.Polygons;
using RedAlertLEDs.Services.Predictor;
using RedAlertLEDs.Services.StateManager;
using RedAlertLEDs.Services.Tzofar;
using Serilog;

namespace RedAlertLEDs.Extensions;

public static class AppBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddServices()
        {
            builder.Services.AddSingleton<PolygonsService>();
            builder.Services.AddSingleton<PredictorService>();
            builder.Services.AddSingleton<StateManagerService>();
            builder.Services.AddSingleton<LedStripService>();
        }

        public void AddRepositories()
        {
            builder.Services.AddSingleton<PolygonsRepository>();
        }

        public void AddBackgroundServices()
        {
            builder.Services.AddSingleton<TzofarAlertsPoller>();

            builder.Services.AddHostedService(provider =>
                provider.GetRequiredService<TzofarAlertsPoller>());
        }

        public void AddOrchestrators()
        {
            builder.Services.AddSingleton<Orchestrator>();
            builder.Services.AddHostedService<StartupInitializer>();
        }

        public void AddSwagger()
        {
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        public void AddSerilog()
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
}