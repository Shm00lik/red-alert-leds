namespace RedAlertLEDs.Services;

public class StartupInitializer(Orchestrator orchestrator) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await orchestrator.Initialize();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await orchestrator.Stop();
    }
}