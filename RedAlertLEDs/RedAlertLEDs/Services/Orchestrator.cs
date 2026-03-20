using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.Polygons;
using RedAlertLEDs.Services.StateManager;
using RedAlertLEDs.Services.Tzofar;

namespace RedAlertLEDs.Services;

public class Orchestrator(
    TzofarAlertsPoller tzofarAlertsPoller,
    PolygonsService polygonsService,
    StateManagerService stateManagerService,
    LedStripService ledStripService)
{
    public async Task Initialize()
    {
        tzofarAlertsPoller.AlertReceived += polygonsService.OnAlertReceived;
        polygonsService.RelevantAlertReceived += stateManagerService.OnRelevantAlertReceived;
        stateManagerService.AlertStateChanged += ledStripService.OnAlertStateChanged;

        // await ledStripService.TurnOn();
    }

    public async Task Stop()
    {
        tzofarAlertsPoller.AlertReceived -= polygonsService.OnAlertReceived;
        polygonsService.RelevantAlertReceived -= stateManagerService.OnRelevantAlertReceived;
        stateManagerService.AlertStateChanged -= ledStripService.OnAlertStateChanged;

        await ledStripService.TurnOff();
    }
}