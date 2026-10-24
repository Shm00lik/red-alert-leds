using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTnet;
using MQTTnet.Protocol;

namespace RedAlertLEDs;

/// <summary>A command for the ESP LED controller, published as JSON on <c>leds/cmd</c>.</summary>
public record LedCommand(
    string? Effect,
    string? Color = null,
    int? Brightness = null,
    int? PeriodMs = null,
    int? From = null,
    int? To = null,
    int? DurationMs = null,
    int? SpeedMs = null)
{
    public static readonly string[] Effects = ["fill", "blink", "progress", "rainbow", "idle"];

    public static LedCommand Idle { get; } = new("idle");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);
}

public interface ILedPublisher
{
    Task SendAsync(LedCommand command);

    bool MqttConnected { get; }

    bool EspOnline { get; }
}

public class MqttOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string? Username { get; set; }
    public string? Password { get; set; }
}

/// <summary>
/// Keeps an MQTT connection open, publishes LED commands (retained) and tracks whether the ESP is online.
/// Registers a last will that turns the strip off if this server disappears.
/// </summary>
public class MqttLedPublisher(MqttOptions options, ILogger<MqttLedPublisher> logger)
    : BackgroundService, ILedPublisher
{
    public const string CommandTopic = "leds/cmd";
    public const string StatusTopic = "leds/status";

    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(30);

    private readonly IMqttClient _client = new MqttClientFactory().CreateMqttClient();
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private LedCommand? _lastCommand;

    public bool MqttConnected => _client.IsConnected;

    public bool EspOnline { get; private set; }

    public async Task SendAsync(LedCommand command)
    {
        _lastCommand = command;

        if (!_client.IsConnected)
        {
            logger.LogWarning("MQTT not connected, will send {Command} after reconnect", command.ToJson());
            return;
        }

        await PublishAsync(command, CancellationToken.None);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.ApplicationMessageReceivedAsync += e =>
        {
            if (e.ApplicationMessage.Topic == StatusTopic)
            {
                EspOnline = e.ApplicationMessage.ConvertPayloadToString() == "online";
                logger.LogInformation("ESP is {Status}", EspOnline ? "online" : "offline");
            }

            return Task.CompletedTask;
        };

        var backoff = TimeSpan.FromSeconds(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_client.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            try
            {
                logger.LogInformation("Connecting to MQTT broker {Host}:{Port}...", options.Host, options.Port);

                await _client.ConnectAsync(BuildOptions(), stoppingToken);
                await _client.SubscribeAsync(
                    new MqttClientSubscribeOptionsBuilder()
                        .WithTopicFilter(StatusTopic, MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build(),
                    stoppingToken);

                logger.LogInformation("MQTT connected");
                backoff = TimeSpan.FromSeconds(1);

                // The broker may have published our last will (idle) while we were away.
                if (_lastCommand != null)
                {
                    await PublishAsync(_lastCommand, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning("MQTT connection failed ({Message}). Retrying in {Seconds}s",
                    e.Message, backoff.TotalSeconds);

                await Task.Delay(backoff, stoppingToken);
                backoff = TimeSpan.FromTicks(Math.Min(backoff.Ticks * 2, MaxBackoff.Ticks));
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_client.IsConnected)
        {
            // A clean disconnect does not trigger the last will, so turn the strip off explicitly.
            await PublishAsync(LedCommand.Idle, cancellationToken);
            await _client.DisconnectAsync(cancellationToken: cancellationToken);
        }
    }

    private MqttClientOptions BuildOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(options.Host, options.Port)
            .WithClientId($"redalert-{Environment.MachineName}")
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))
            .WithWillTopic(CommandTopic)
            .WithWillPayload(LedCommand.Idle.ToJson())
            .WithWillRetain()
            .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce);

        if (!string.IsNullOrEmpty(options.Username))
        {
            builder.WithCredentials(options.Username, options.Password);
        }

        return builder.Build();
    }

    private async Task PublishAsync(LedCommand command, CancellationToken ct)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(CommandTopic)
            .WithPayload(command.ToJson())
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();

        await _publishLock.WaitAsync(ct);

        try
        {
            await _client.PublishAsync(message, ct);
            logger.LogInformation("Sent {Command}", command.ToJson());
        }
        catch (Exception e)
        {
            logger.LogWarning("Failed to publish {Command}: {Message}", command.ToJson(), e.Message);
        }
        finally
        {
            _publishLock.Release();
        }
    }
}
