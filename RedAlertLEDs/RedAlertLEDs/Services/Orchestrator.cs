using RedAlertLEDs.Services.HomeFrontCommand;
using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.Polygons;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services;

public class Orchestrator(
    HomeFrontCommandPoller homeFrontCommandPoller,
    PolygonsService polygonsService,
    StateManagerService stateManagerService,
    LedStripService ledStripService)
{
    public async Task Initialize()
    {
        homeFrontCommandPoller.AlertReceived += polygonsService.OnAlertReceived;
        polygonsService.RelevantAlertReceived += stateManagerService.OnRelevantAlertReceived;
        stateManagerService.AlertStateChanged += ledStripService.OnAlertStateChanged;

        await ledStripService.TurnOn();
    }

    public async Task Stop()
    {
        homeFrontCommandPoller.AlertReceived -= polygonsService.OnAlertReceived;
        polygonsService.RelevantAlertReceived -= stateManagerService.OnRelevantAlertReceived;
        stateManagerService.AlertStateChanged -= ledStripService.OnAlertStateChanged;

        await ledStripService.TurnOff();
    }
}