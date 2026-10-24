using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace RedAlertLEDs.Tests;

public class AlertEngineTests
{
    private const int TelAviv = 1;
    private const int Haifa = 2;

    private static readonly DateTime T0 = new(2026, 10, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(T0));
    private readonly FakePublisher _publisher = new();
    private readonly Settings _settings = new(SettingsData.Default with { CityIds = [TelAviv], Brightness = 128 });
    private readonly AlertEngine _engine;

    public AlertEngineTests()
    {
        _engine = new AlertEngine(_settings, _publisher, _time, NullLogger<AlertEngine>.Instance);
    }

    [Fact]
    public async Task RelevantCityTriggers()
    {
        Assert.True(await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [Haifa, TelAviv])));
        Assert.Equal(LedState.Alert, _engine.State);
        Assert.Single(_publisher.Sent);
    }

    [Fact]
    public async Task IrrelevantCityIsIgnored()
    {
        Assert.False(await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [Haifa])));
        Assert.Equal(LedState.Idle, _engine.State);
        Assert.Empty(_publisher.Sent);
        Assert.Empty(_engine.Recent);
    }

    [Fact]
    public async Task EmptyCityListMeansEverywhere()
    {
        _settings.Update(_settings.Current with { CityIds = [] });

        Assert.True(await _engine.HandleAsync(new Alert(T0, AlertKind.Uav, [Haifa])));
        Assert.Equal(LedState.Alert, _engine.State);
    }

    [Theory]
    [InlineData(AlertKind.EarlyWarning, LedState.EarlyWarning, "#FF4500", 600_000)]
    [InlineData(AlertKind.Missiles, LedState.Alert, "#FF0000", 600_000)]
    [InlineData(AlertKind.Uav, LedState.Alert, "#FF0000", 600_000)]
    [InlineData(AlertKind.Ended, LedState.Safe, "#00FF00", 60_000)]
    public async Task KindMapsToProgressCommand(AlertKind kind, LedState state, string color, int durationMs)
    {
        await _engine.HandleAsync(new Alert(T0, kind, [TelAviv]));

        Assert.Equal(state, _engine.State);
        Assert.Equal(
            new LedCommand("progress", color, Brightness: 128, From: 100, To: 0, DurationMs: durationMs),
            _publisher.Sent.Single());
        Assert.Equal(_time.GetUtcNow().AddMilliseconds(durationMs), _engine.CountdownEndsAt);
    }

    [Fact]
    public async Task UnknownKindIsIgnored()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [TelAviv]));

        Assert.False(await _engine.HandleAsync(new Alert(T0.AddSeconds(1), AlertKind.Unknown, [TelAviv])));
        Assert.Equal(LedState.Alert, _engine.State);
        Assert.Single(_publisher.Sent);
    }

    [Fact]
    public async Task DuplicateIsIgnored()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [TelAviv]));

        Assert.False(await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [TelAviv])));
        Assert.Single(_publisher.Sent);
        Assert.Single(_engine.Recent);
    }

    [Fact]
    public async Task RepeatOfSameStateRestartsCountdown()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [TelAviv]));
        _time.Advance(TimeSpan.FromMinutes(3));

        Assert.True(await _engine.HandleAsync(new Alert(T0.AddMinutes(3), AlertKind.Missiles, [TelAviv])));
        Assert.Equal(2, _publisher.Sent.Count);
        Assert.Equal(_time.GetUtcNow() + AlertEngine.AlertDuration, _engine.CountdownEndsAt);
    }

    [Fact]
    public async Task SafeTurnsIdleAfterOneMinute()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Ended, [TelAviv]));

        _time.Advance(TimeSpan.FromSeconds(59));
        Assert.Equal(LedState.Safe, _engine.State);

        _time.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(LedState.Idle, _engine.State);
        Assert.Equal("idle", _publisher.Sent.Last().Effect);
        Assert.Null(_engine.CountdownEndsAt);
    }

    [Fact]
    public async Task NewAlertCancelsPendingIdle()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Ended, [TelAviv]));
        _time.Advance(TimeSpan.FromSeconds(30));
        await _engine.HandleAsync(new Alert(T0.AddSeconds(30), AlertKind.Missiles, [TelAviv]));

        _time.Advance(TimeSpan.FromMinutes(2));

        Assert.Equal(LedState.Alert, _engine.State);
        Assert.DoesNotContain(_publisher.Sent, c => c.Effect == "idle");
    }

    [Fact]
    public async Task ManualIsOverriddenByNextAlert()
    {
        await _engine.SetManualAsync(new LedCommand("rainbow"));
        Assert.Equal(LedState.Manual, _engine.State);
        Assert.Equal(new LedCommand("rainbow", Brightness: 128), _publisher.Sent.Last());

        await _engine.HandleAsync(new Alert(T0, AlertKind.EarlyWarning, [TelAviv]));
        Assert.Equal(LedState.EarlyWarning, _engine.State);
    }

    [Fact]
    public async Task ForceIdleSendsIdle()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [TelAviv]));
        await _engine.ForceStateAsync(LedState.Idle);

        Assert.Equal(LedState.Idle, _engine.State);
        Assert.Equal(new LedCommand("idle", Brightness: 128), _publisher.Sent.Last());
    }

    [Fact]
    public async Task ResendKeepsRemainingCountdownWithNewSettings()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Missiles, [TelAviv]));
        _time.Advance(TimeSpan.FromMinutes(5));

        _settings.Update(_settings.Current with
        {
            Brightness = 50,
            Colors = _settings.Current.Colors with { Alert = "#AA0000" }
        });
        await _engine.ResendCurrentAsync();

        Assert.Equal(
            new LedCommand("progress", "#AA0000", Brightness: 50, From: 50, To: 0, DurationMs: 300_000),
            _publisher.Sent.Last());
    }

    [Fact]
    public async Task EveryCommandCarriesBrightness()
    {
        await _engine.HandleAsync(new Alert(T0, AlertKind.Ended, [TelAviv]));
        _time.Advance(AlertEngine.SafeDuration);
        await _engine.SetManualAsync(new LedCommand("fill", "#123456"));

        Assert.All(_publisher.Sent, c => Assert.Equal(128, c.Brightness));
    }

    [Fact]
    public async Task RecentKeepsLatestTwentyNewestFirst()
    {
        for (var i = 0; i < 25; i++)
        {
            await _engine.HandleAsync(new Alert(T0.AddMinutes(i), AlertKind.Missiles, [TelAviv]));
        }

        Assert.Equal(20, _engine.Recent.Count);
        Assert.Equal(T0.AddMinutes(24), _engine.Recent[0].Time);
    }

    private class FakePublisher : ILedPublisher
    {
        public List<LedCommand> Sent { get; } = [];

        public Task SendAsync(LedCommand command)
        {
            Sent.Add(command);
            return Task.CompletedTask;
        }

        public bool MqttConnected => true;

        public bool EspOnline => true;
    }
}
