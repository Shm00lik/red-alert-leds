namespace RedAlertLEDs;

/// <summary>
/// Decides what the LED strip shows: filters alerts by the user's cities, maps them to a state and
/// publishes one LED command per state change. The ESP animates countdowns by itself.
/// </summary>
public class AlertEngine(
    Settings settings,
    ILedPublisher publisher,
    TimeProvider time,
    ILogger<AlertEngine> logger)
{
    public static readonly TimeSpan EarlyWarningDuration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan AlertDuration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan SafeDuration = TimeSpan.FromMinutes(1);

    private const int RecentCapacity = 20;

    private readonly Lock _lock = new();
    private readonly LinkedList<RecentAlert> _recent = new();
    private (DateTime Time, AlertKind Kind)? _lastHandled;
    private ITimer? _idleTimer;
    private LedCommand? _manualCommand;

    public LedState State { get; private set; } = LedState.Idle;

    public DateTimeOffset StateSince { get; private set; } = time.GetUtcNow();

    public DateTimeOffset? CountdownEndsAt { get; private set; }

    public IReadOnlyList<RecentAlert> Recent
    {
        get
        {
            lock (_lock)
            {
                return _recent.ToList();
            }
        }
    }

    /// <summary>Handles an alert from Tzofar. Returns true if it changed what the strip shows.</summary>
    public async Task<bool> HandleAsync(Alert alert)
    {
        if (alert.Kind == AlertKind.Unknown)
        {
            logger.LogInformation("Ignoring alert of unknown kind at {Time}", alert.Time);
            return false;
        }

        LedCommand command;

        lock (_lock)
        {
            if (_lastHandled == (alert.Time, alert.Kind))
            {
                return false;
            }

            if (!IsRelevant(alert))
            {
                return false;
            }

            _lastHandled = (alert.Time, alert.Kind);

            _recent.AddFirst(new RecentAlert(alert.Time, alert.Kind, alert.CityIds));

            if (_recent.Count > RecentCapacity)
            {
                _recent.RemoveLast();
            }

            command = Apply(StateFor(alert.Kind));
        }

        logger.LogInformation("Relevant {Kind} alert → {State}", alert.Kind, State);

        await publisher.SendAsync(command);
        return true;
    }

    /// <summary>Shows a state as if a matching alert had arrived (for testing from the control page).</summary>
    public Task ForceStateAsync(LedState state)
    {
        if (state == LedState.Manual)
        {
            throw new ArgumentException("Use SetManualAsync for manual commands", nameof(state));
        }

        LedCommand command;

        lock (_lock)
        {
            command = Apply(state);
        }

        return publisher.SendAsync(command);
    }

    /// <summary>Sends a command from the control page. The next relevant alert overrides it.</summary>
    public Task SetManualAsync(LedCommand manual)
    {
        LedCommand command;

        lock (_lock)
        {
            CancelIdleTimer();
            _manualCommand = manual;
            command = manual with { Brightness = manual.Brightness ?? settings.Current.Brightness };
            SetState(LedState.Manual, null);
        }

        return publisher.SendAsync(command);
    }

    /// <summary>Re-sends the current state with the latest colours and brightness, keeping its countdown.</summary>
    public Task ResendCurrentAsync()
    {
        LedCommand command;

        lock (_lock)
        {
            command = State switch
            {
                LedState.Manual => _manualCommand! with { Brightness = settings.Current.Brightness },
                LedState.Idle => LedCommand.Idle with { Brightness = settings.Current.Brightness },
                _ => ProgressFor(State, RemainingFraction())
            };
        }

        return publisher.SendAsync(command);
    }

    private bool IsRelevant(Alert alert)
    {
        var cityIds = settings.Current.CityIds;

        return cityIds.Count == 0 || alert.CityIds.Any(cityIds.Contains);
    }

    private static LedState StateFor(AlertKind kind) => kind switch
    {
        AlertKind.EarlyWarning => LedState.EarlyWarning,
        AlertKind.Missiles or AlertKind.Uav => LedState.Alert,
        AlertKind.Ended => LedState.Safe,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    // Must be called under _lock.
    private LedCommand Apply(LedState state)
    {
        CancelIdleTimer();

        if (state == LedState.Idle)
        {
            SetState(LedState.Idle, null);
            return LedCommand.Idle with { Brightness = settings.Current.Brightness };
        }

        var duration = DurationFor(state);
        SetState(state, time.GetUtcNow() + duration);

        if (state == LedState.Safe)
        {
            _idleTimer = time.CreateTimer(_ => OnSafeEnded(), null, duration, Timeout.InfiniteTimeSpan);
        }

        return ProgressFor(state, 1.0);
    }

    private void OnSafeEnded()
    {
        LedCommand command;

        lock (_lock)
        {
            if (State != LedState.Safe)
            {
                return;
            }

            command = Apply(LedState.Idle);
        }

        _ = publisher.SendAsync(command);
    }

    private LedCommand ProgressFor(LedState state, double remainingFraction)
    {
        var current = settings.Current;
        var color = state switch
        {
            LedState.EarlyWarning => current.Colors.EarlyWarning,
            LedState.Alert => current.Colors.Alert,
            _ => current.Colors.Safe
        };

        var duration = DurationFor(state) * remainingFraction;

        return new LedCommand(
            Effect: "progress",
            Color: color,
            Brightness: current.Brightness,
            From: (int)Math.Round(remainingFraction * 100),
            To: 0,
            DurationMs: (int)duration.TotalMilliseconds);
    }

    private double RemainingFraction()
    {
        if (CountdownEndsAt is not { } endsAt)
        {
            return 1.0;
        }

        var remaining = endsAt - time.GetUtcNow();
        return Math.Clamp(remaining / DurationFor(State), 0, 1);
    }

    private static TimeSpan DurationFor(LedState state) => state switch
    {
        LedState.EarlyWarning => EarlyWarningDuration,
        LedState.Alert => AlertDuration,
        LedState.Safe => SafeDuration,
        _ => TimeSpan.Zero
    };

    private void SetState(LedState state, DateTimeOffset? countdownEndsAt)
    {
        State = state;
        StateSince = time.GetUtcNow();
        CountdownEndsAt = countdownEndsAt;
    }

    private void CancelIdleTimer()
    {
        _idleTimer?.Dispose();
        _idleTimer = null;
    }
}
