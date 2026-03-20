using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using RedAlertLEDs.BO.Alerts;
using RedAlertLEDs.BO.TzofarMessages;
using ILogger = Serilog.ILogger;

namespace RedAlertLEDs.Services.Tzofar;

public class TzofarAlertsPoller(ILogger logger) : BackgroundService
{
    private const int WebsocketMessageBufferSize = 4096;

    private readonly Uri _wsUri = new("wss://ws.tzevaadom.co.il/socket?platform=WEB");

    public event EventHandler<AlertEventArgs>? AlertReceived;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var socket = new ClientWebSocket();

            socket.Options.SetRequestHeader("Origin", "https://www.tzevaadom.co.il");

            try
            {
                await socket.ConnectAsync(_wsUri, stoppingToken);

                logger.Information("Websocket connected!");

                await ReceiveLoop(socket, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.Information("Websocket connection cancelled.");
            }
            catch (Exception e)
            {
                logger.Error(e, "Websocket connection exception!");
            }
        }
    }

    private async Task ReceiveLoop(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[WebsocketMessageBufferSize];

        while (ShouldReadMessages(socket, ct))
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult? result = null;

            while (result is not { EndOfMessage: true })
            {
                result = await socket.ReceiveAsync(buffer, ct);

                if (ShouldCloseConnection(result))
                {
                    await CloseConnection(socket, ct);
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            }

            var messageBytes = ms.ToArray();
            var message = System.Text.Encoding.UTF8.GetString(messageBytes);

            if (string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            HandleMessage(message);
        }

        logger.Warning("Receive loop stopped unexpectedly!");
    }

    private void HandleMessage(string rawMessage)
    {
        var message = JsonSerializer.Deserialize<TzofarMessage>(rawMessage);

        if (message == null)
        {
            return;
        }

        var alert = message.Type switch
        {
            "ALERT" => ConvertAlertMessageDataToAlert(message.Data),
            "SYSTEM_MESSAGE" => ConvertSystemMessageDataToAlert(message.Data),
            _ => null
        };

        if (alert == null)
        {
            return;
        }

        OnAlertReceived(alert);
    }

    private Alert ConvertAlertMessageDataToAlert(JsonObject data)
    {
        var alertDateTime = DateTimeOffset
            .FromUnixTimeSeconds(data["time"]?.GetValue<long>() ?? 0)
            .LocalDateTime;

        var alertCategory = data["threat"]?.ToString() switch
        {
            "0" => AlertCategory.Missiles,
            "5" => AlertCategory.Uav,
            _ => AlertCategory.Unknown
        };

        var alertPolygons = data["cities"]?.Deserialize<List<string>>() ?? [];

        return new Alert
        {
            Time = alertDateTime,
            Category = alertCategory,
            Polygons = alertPolygons
        };
    }

    private Alert ConvertSystemMessageDataToAlert(JsonObject data)
    {
        var alertDateTime = DateTimeOffset
            .FromUnixTimeSeconds(long.Parse(data["time"]?.GetValue<string>() ?? string.Empty))
            .LocalDateTime;

        var alertCategory = data["instructionType"]?.ToString() switch
        {
            "0" => AlertCategory.EarlyWarning,
            "1" => AlertCategory.IncidentEnded,
            _ => AlertCategory.Unknown
        };

        var alertPolygonsIds = data["citiesIds"]?.Deserialize<List<int>>() ?? [];
        var alertPolygons = alertPolygonsIds.Select(id => id.ToString()).ToList();
        
        return new Alert
        {
            Time = alertDateTime,
            Category = alertCategory,
            Polygons = alertPolygons
        };
    }

    private static bool ShouldReadMessages(ClientWebSocket socket, CancellationToken ct)
    {
        return socket.State == WebSocketState.Open && !ct.IsCancellationRequested;
    }

    private static bool ShouldCloseConnection(WebSocketReceiveResult result)
    {
        return result.MessageType == WebSocketMessageType.Close;
    }

    private async Task CloseConnection(ClientWebSocket socket, CancellationToken ct)
    {
        logger.Information("Closing connection!");
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
        logger.Information("Conneciton closed!");
    }

    private void OnAlertReceived(Alert alert)
    {
        AlertReceived?.Invoke(this, new AlertEventArgs
        {
            Alert = alert
        });
    }
}