using RedAlertLEDs.BO;
using RedAlertLEDs.BO.Alerts;
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

        SetState(GetStateForAlert(e.Alert), this);
    }

    public void SetState(AlertState state, object? sender)
    {
        AlertStateChanged?.Invoke(sender, new AlertStateChangedEventArgs
        {
            State = state
        });

        _currentState = state;
    }

    public AlertState GetState()
    {
        return _currentState;
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