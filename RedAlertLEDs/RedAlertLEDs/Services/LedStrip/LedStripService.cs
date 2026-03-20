using System.Drawing;
using RedAlertLEDs.Hardware;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services.LedStrip;

public class LedStripService
{
    public const int NumOfLeds = 86;

    private readonly ArduinoLedStrip _ledStrip = new(NumOfLeds);

    public void OnAlertStateChanged(object? sender, AlertStateChangedEventArgs e)
    {
        var color = GetStateColor(e.CurrentState);
        _ledStrip.SetColor(color, true);
    }

    public void SetLedColor(Color color, int index, bool update = false)
    {
        _ledStrip.SetColor(color, index);

        if (update)
        {
            _ledStrip.Update();
        }
    }

    public void SetColor(Color color, bool update = false) {
        _ledStrip.SetColor(color, true);
    }

    public async Task TurnOn()
    {
        var noneColor = GetStateColor(AlertState.None);
        var safeColor = GetStateColor(AlertState.Safe);

        await Blink(safeColor, noneColor, 150);
    }

    public async Task TurnOff()
    {
        var noneColor = GetStateColor(AlertState.None);
        var safeColor = GetStateColor(AlertState.Safe);

        await Blink(safeColor, noneColor, 150);
    }

    private async Task Blink(Color color1, Color color2, int intervalMs)
    {
        _ledStrip.SetColor(color1, true);

        await Task.Delay(intervalMs);

        _ledStrip.SetColor(color2, true);

        await Task.Delay(intervalMs);

        _ledStrip.SetColor(color1, true);

        await Task.Delay(intervalMs);

        _ledStrip.SetColor(color2, true);
    }

    public static Color GetStateColor(AlertState state)
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