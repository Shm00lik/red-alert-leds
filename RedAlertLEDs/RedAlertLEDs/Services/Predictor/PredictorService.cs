using System.Diagnostics;
using System.Drawing;
using RedAlertLEDs.Services.LedStrip;
using RedAlertLEDs.Services.StateManager;

namespace RedAlertLEDs.Services.Predictor;

public class PredictorService(
    StateManagerService stateManagerService,
    LedStripService ledStripService
)
{
    private CancellationTokenSource _cts = new();

    private readonly TimeSpan _safeToNoneInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _earlyWarningToAlertInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _alertToSafeInterval = TimeSpan.FromSeconds(10);

    public void OnAlertStateChanged(object? sender, AlertStateChangedEventArgs e)
    {
        if (sender == this)
        {
            return;
        }

        ResetCancellationToken();

        switch (e.CurrentState)
        {
            case AlertState.EarlyWarning:
                _ = HandleEarlyWarning(_cts.Token);
                break;
            case AlertState.Alert:
                _ = HandleAlert(_cts.Token);
                break;
            case AlertState.Safe:
                _ = HandleSafe(_cts.Token);
                break;
            case AlertState.None:
            default:
                break;
        }
    }

    private async Task HandleEarlyWarning(CancellationToken ct)
    {
        var intervalBetweenLeds = _earlyWarningToAlertInterval / LedStripService.NumOfLeds;

        for (var i = 0; i < LedStripService.NumOfLeds; i++)
        {
            await Task.Delay(intervalBetweenLeds, ct);
            ledStripService.SetLedColor(Color.Black, i, true);
        }
    }

    private async Task HandleAlert(CancellationToken ct)
    {
        var intervalBetweenLeds = _alertToSafeInterval / LedStripService.NumOfLeds;

        for (var i = 0; i < LedStripService.NumOfLeds; i++)
        {
            await Task.Delay(intervalBetweenLeds, ct);
            ledStripService.SetLedColor(Color.Black, i, true);
        }
    }

    private async Task HandleSafe(CancellationToken ct)
    {
        var intervalBetweenLeds = _safeToNoneInterval / LedStripService.NumOfLeds;

        for (var i = 0; i < LedStripService.NumOfLeds; i++)
        {
            await Task.Delay(intervalBetweenLeds, ct);
            ledStripService.SetLedColor(Color.Black, i, true);
        }
    }

    private void ResetCancellationToken()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
    }
}