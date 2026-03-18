using RedAlertLEDs.BO;
using RedAlertLEDs.Services.Polygons;
using ILogger = Serilog.ILogger;

namespace RedAlertLEDs.Services.StateManager;

public class StateManagerService(ILogger logger)
{
    private AlertState _currentState = AlertState.None;

    public event EventHandler<AlertStateChangedEventArgs>? AlertStateChanged;

    public void OnRelevantAlertReceived(object? sender, RelevantAlertEventArgs e)
    {
        var newState = GetStateForAlert(e.Alert);

        logger.Debug("New state: {NewState}. Current state: {CurrentState}", newState, _currentState);

        if (newState == _currentState)
        {
            return;
        }

        SetState(GetStateForAlert(e.Alert), sender);
    }

    private void SetState(AlertState state, object? sender)
    {
        AlertStateChanged?.Invoke(sender, new AlertStateChangedEventArgs
        {
            PreviousState = _currentState,
            CurrentState = state
        });

        _currentState = state;
    }

    private static AlertState GetStateForAlert(HistoricalAlert alert)
    {
        return alert.Category switch
        {
            AlertCategory.EarlyWarning => AlertState.EarlyWarning,
            AlertCategory.Missiles or AlertCategory.Uav => AlertState.Alert,
            AlertCategory.IncidentEnded => AlertState.Safe,
            _ => AlertState.None
        };
    }
}