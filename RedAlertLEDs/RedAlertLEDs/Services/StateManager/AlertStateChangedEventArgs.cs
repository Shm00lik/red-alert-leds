namespace RedAlertLEDs.Services.StateManager;

public class AlertStateChangedEventArgs : EventArgs
{
    public AlertState PreviousState { get; init; }
    public AlertState CurrentState { get; init; }
}