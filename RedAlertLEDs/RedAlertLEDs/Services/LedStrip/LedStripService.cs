using System.Drawing;
using RedAlertLEDs.Hardware;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services.LedStrip;

public class LedStripService
{
    private const int NumOfLeds = 86;

    private readonly ArduinoLedStrip _ledStrip = new(NumOfLeds);

    public void OnAlertStateChanged(object? sender, AlertStateChangedEventArgs e)
    {
        var color = GetStateColor(e.CurrentState);
        _ledStrip.SetColor(color, true);
    }

    public async Task TurnOff()
    {
        var noneColor = GetStateColor(AlertState.None);
        var safeColor = GetStateColor(AlertState.Safe);


        for (var i = 0; i < NumOfLeds; i++)
        {
            _ledStrip.SetColor(noneColor, i);
            _ledStrip.Update();
            await Task.Delay(10);
        }

        _ledStrip.SetColor(noneColor, true);

        await Task.Delay(150);

        _ledStrip.SetColor(safeColor, true);

        await Task.Delay(150);

        _ledStrip.SetColor(noneColor, true);

        await Task.Delay(150);

        _ledStrip.SetColor(safeColor, true);

        await Task.Delay(150);

        _ledStrip.SetColor(noneColor, true);
    }

    private static Color GetStateColor(AlertState state)
    {
        switch (state)
        {
            case AlertState.EarlyWarning:
                return Color.OrangeRed;
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