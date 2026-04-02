using System.Drawing;
using RedAlertLEDs.Hardware;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services.LedStrip;

public class LedStripService
{
    public const int NumOfLeds = 86;
    private const int OpenDelay = 2000;

    private readonly ArduinoLedStrip _ledStrip = new(NumOfLeds, Arduino.DefaultPort);
    private bool _isReady;
    private double _colorMultiplier = 1;

    public void OnAlertStateChanged(object? sender, AlertStateChangedEventArgs e)
    {
        if (!_isReady)
        {
            return;
        }

        var color = GetStateColor(e.State);
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

    public void SetColor(Color color, bool update = false)
    {
        _ledStrip.SetColor(color, true);
    }

    public void SetColorMultiplier(double multiplier)
    {
        _colorMultiplier = multiplier;
    }

    public double GetColorMultiplier()
    {
        return _colorMultiplier;
    }

    public async Task TurnOn()
    {
        await Task.Delay(OpenDelay);

        var noneColor = GetStateColor(AlertState.None);
        var safeColor = GetStateColor(AlertState.Safe);

        await Blink(safeColor, noneColor, 150);

        _isReady = true;
    }

    public async Task TurnOff()
    {
        var noneColor = GetStateColor(AlertState.None);
        var safeColor = GetStateColor(AlertState.Safe);

        await Blink(safeColor, noneColor, 150);

        _isReady = false;
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

    public Color GetStateColor(AlertState state)
    {
        var color = state switch
        {
            AlertState.EarlyWarning => Color.OrangeRed,
            AlertState.Alert => Color.Red,
            AlertState.Safe => Color.Green,
            _ => Color.Black // Also for AlertState.None
        };

        return Color.FromArgb(
            (int)(color.R * _colorMultiplier),
            (int)(color.G * _colorMultiplier),
            (int)(color.B * _colorMultiplier)
        );
    }
}