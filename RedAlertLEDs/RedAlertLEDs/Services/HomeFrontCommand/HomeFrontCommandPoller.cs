using System.Text.Json;
using System.Text.Json.Serialization;
using RedAlertLEDs.BO;
using RedAlertLEDs.Extensions;
using RedAlertLEDs.JsonConverters;
using RedAlertLEDs.Services.Polygons;
using ILogger = Serilog.ILogger;

namespace RedAlertLEDs.Services.HomeFrontCommand;

public class HomeFrontCommandPoller(ILogger logger) : BackgroundService
{
    private const string CurrentAlertsUrl = "https://www.oref.org.il/warningMessages/alert/Alerts.json";
    private const string AlertsHistoryUrl = "https://www.oref.org.il/warningMessages/alert/History/AlertsHistory.json";

    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);

    private readonly HttpClient _httpClient = new();

    public event EventHandler<AlertEventArgs>? AlertReceived;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var alerts = (await GetHistoricalAlerts()).ToList();

            logger.Information("Received {Count} alerts", alerts.Count);

            foreach (var alert in alerts)
            {
                Console.WriteLine(alert);
                OnAlertReceived(alert);
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task<IEnumerable<Alert>> GetCurrentAlerts()
    {
        var response = await _httpClient.GetAsync(CurrentAlertsUrl);

        response.EnsureSuccessStatusCode();

        try
        {
            var data = await response.Content.ReadOneOrMoreAsync<Alert>();
            return data;
        }
        catch (JsonException)
        {
            var json = await response.Content.ReadAsStringAsync();
            logger.Warning("Failed to deserialize alerts response: {Json}", json);

            return [];
        }
    }

    private async Task<IEnumerable<HistoricalAlert>> GetHistoricalAlerts()
    {
        var response = await _httpClient.GetAsync(AlertsHistoryUrl);

        response.EnsureSuccessStatusCode();

        try
        {
            var data = await response.Content.ReadOneOrMoreAsync<HistoricalAlert>();
            return data.Where(alert => alert.Date > DateTime.Now - TimeSpan.FromHours(1)).OrderBy(alert => alert.Date);
        }
        catch (JsonException ex)
        {
            var json = await response.Content.ReadAsStringAsync();
            logger.Warning(
                "Failed to deserialize historical alerts response: {Message}\n{Json}",
                ex,
                string.Join("", json.Take(200))
            );

            return [];
        }
    }

    private void OnAlertReceived(HistoricalAlert alert)
    {
        AlertReceived?.Invoke(this, new AlertEventArgs
        {
            Alert = alert
        });
    }
}