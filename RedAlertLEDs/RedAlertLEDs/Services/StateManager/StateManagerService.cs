using RedAlertLEDs.BO;
using RedAlertLEDs.Services.Polygons;

namespace RedAlertLEDs.Services.StateManager;

public class StateManagerService
{
    private AlertState _currentState = AlertState.None;

    public event EventHandler<AlertStateChangedEventArgs>? AlertStateChanged;

    public void OnRelevantAlertReceived(object? sender, RelevantAlertEventArgs e)
    {
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

    private static AlertState GetStateForAlert(Alert alert)
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