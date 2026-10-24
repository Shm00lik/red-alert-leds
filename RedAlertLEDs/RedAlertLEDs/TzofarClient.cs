using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RedAlertLEDs;

/// <summary>Listens to the Tzofar (tzevaadom) websocket and passes every alert to the <see cref="AlertEngine"/>.</summary>
public class TzofarClient(AlertEngine engine, Cities cities, ILogger<TzofarClient> logger) : BackgroundService
{
    private const int BufferSize = 4096;

    private static readonly Uri WsUri = new("wss://ws.tzevaadom.co.il/socket?platform=WEB");
    private static readonly TimeSpan MinBackoff = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromMinutes(5);

    public bool Connected { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var backoff = MinBackoff;

        while (!stoppingToken.IsCancellationRequested)
        {
            using var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Origin", "https://www.tzevaadom.co.il");

            try
            {
                logger.LogInformation("Connecting to Tzofar...");
                await socket.ConnectAsync(WsUri, stoppingToken);

                Connected = true;
                backoff = MinBackoff;
                logger.LogInformation("Tzofar connected");

                await ReceiveLoop(socket, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning("Tzofar connection failed: {Message}", e.Message);
            }
            finally
            {
                Connected = false;
            }

            logger.LogInformation("Reconnecting to Tzofar in {Seconds}s", backoff.TotalSeconds);
            await Task.Delay(backoff, stoppingToken);
            backoff = TimeSpan.FromTicks(Math.Min(backoff.Ticks * 2, MaxBackoff.Ticks));
        }
    }

    private async Task ReceiveLoop(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[BufferSize];

        while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await socket.ReceiveAsync(buffer, ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.LogInformation("Tzofar closed the connection");
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            var message = Encoding.UTF8.GetString(ms.ToArray());

            if (string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            try
            {
                var alert = Parse(message, cities, logger);

                if (alert != null)
                {
                    await engine.HandleAsync(alert);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to handle Tzofar message: {Message}", message);
            }
        }
    }

    /// <summary>Turns a raw Tzofar message into an <see cref="Alert"/>, or null for message types we don't use.</summary>
    public static Alert? Parse(string rawMessage, Cities cities, ILogger logger)
    {
        var message = JsonSerializer.Deserialize<TzofarMessage>(rawMessage);

        if (message?.Data == null)
        {
            return null;
        }

        return message.Type switch
        {
            "ALERT" => ParseAlert(message.Data, cities, logger),
            "SYSTEM_MESSAGE" => ParseSystemMessage(message.Data),
            _ => null
        };
    }

    private static Alert ParseAlert(JsonObject data, Cities cities, ILogger logger)
    {
        var time = FromUnixSeconds(data["time"]);

        var kind = data["threat"]?.ToString() switch
        {
            "0" => AlertKind.Missiles,
            "5" => AlertKind.Uav,
            _ => AlertKind.Unknown
        };

        var names = data["cities"]?.Deserialize<List<string>>() ?? [];
        var ids = new List<int>();

        foreach (var name in names)
        {
            if (cities.IdForName(name) is { } id)
            {
                ids.Add(id);
            }
            else
            {
                logger.LogWarning("Unknown city name in alert: {Name}", name);
            }
        }

        return new Alert(time, kind, ids);
    }

    private static Alert ParseSystemMessage(JsonObject data)
    {
        var time = FromUnixSeconds(data["time"]);

        var kind = data["instructionType"]?.ToString() switch
        {
            "0" => AlertKind.EarlyWarning,
            "1" => AlertKind.Ended,
            _ => AlertKind.Unknown
        };

        var ids = data["citiesIds"]?.Deserialize<List<int>>() ?? [];

        return new Alert(time, kind, ids);
    }

    // ALERT sends the time as a number, SYSTEM_MESSAGE as a string.
    private static DateTime FromUnixSeconds(JsonNode? node)
    {
        var seconds = long.TryParse(node?.ToString(), out var s) ? s : 0;
        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }

    private record TzofarMessage(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("data")] JsonObject? Data);
}
