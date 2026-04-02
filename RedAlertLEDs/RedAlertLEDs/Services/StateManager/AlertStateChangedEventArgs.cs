namespace RedAlertLEDs.Services.StateManager;

public class AlertStateChangedEventArgs : EventArgs
{
    public AlertState State { get; init; }
}