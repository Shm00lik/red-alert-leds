namespace RedAlertLEDs;

public enum AlertKind
{
    Missiles,
    Uav,
    EarlyWarning,
    Ended,
    Unknown
}

public enum LedState
{
    Idle,
    EarlyWarning,
    Alert,
    Safe,
    Manual
}

public record Alert(DateTime Time, AlertKind Kind, IReadOnlyList<int> CityIds);

public record RecentAlert(DateTime Time, AlertKind Kind, IReadOnlyList<int> CityIds);
