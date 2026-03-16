using RedAlertLEDs.Services.Polygons;

namespace RedAlertLEDs.Services.StateManager;

public class StateManagerService
{
    private AlertState _currentState = AlertState.None;
    private readonly LedStripService _ledStripService;

    public event EventHandler<AlertStateChangedEventArgs>? AlertStateChanged;

    public StateManagerService(PolygonsService polygonsService, LedStripService ledStripService)
    {
        polygonsService.RelevantAlertReceived += OnRelevantAlertReceived;
        _ledStripService = ledStripService;
    }

    private void OnRelevantAlertReceived(object? sender, RelevantAlertEventArgs e)
    {
        SetState(AlertState.Alert, sender);
    }

    public void SetState(AlertState state, object? sender)
    {
        AlertStateChanged?.Invoke(sender, new AlertStateChangedEventArgs
        {
            PreviousState = _currentState,
            CurrentState = state
        });

        _currentState = state;
    }
}