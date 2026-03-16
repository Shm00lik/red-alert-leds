using System.Drawing;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services;

public class LedStripService
{
    private const 
        
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