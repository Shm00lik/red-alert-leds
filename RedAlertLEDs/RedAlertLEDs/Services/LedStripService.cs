using System.Drawing;
using RedAlertLEDs.Hardware;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services;

public class LedStripService
{
    private const int NumOfLeds = 86;

    private readonly ArduinoLedStrip _ledStrip = new(NumOfLeds);

    public LedStripService(StateManagerService stateManagerService)
    {
        stateManagerService.AlertStateChanged += OnAlertStateChanged;
    }

    private void OnAlertStateChanged(object? sender, AlertStateChangedEventArgs e)
    {
        var color = GetStateColor(e.CurrentState);
        _ledStrip.SetColor(color, true);
    }

    private Color GetStateColor(AlertState state)
    {
        switch (state)
        {
            case AlertState.EarlyWarning:
                return Color.Orange;
            case AlertState.Alert:
                return Color.Red;
            case AlertState.Safe:
                return Color.Green;
            case AlertState.None:
            default:
                return Color.Black;
        }
    }
}