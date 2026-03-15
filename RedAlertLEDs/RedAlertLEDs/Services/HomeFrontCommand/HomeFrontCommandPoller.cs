using System.Text.Json;
using System.Text.Json.Serialization;
using RedAlertLEDs.BO;

namespace RedAlertLEDs.Services.HomeFrontCommand;

public class HomeFrontCommandPoller : BackgroundService
{
    private const string CurrentAlertsUrl = "https://www.oref.org.il/warningMessages/alert/Alerts.json";
    private const string AlertsHistoryUrl = "https://www.oref.org.il/warningMessages/alert/History/AlertsHistory.json";
    
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient = new();

    public event EventHandler<AlertEventArgs>? AlertReceived;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var alerts = await GetCurrentAlerts();

            foreach (var alert in alerts)
            {
                OnAlertReceived(alert);
            }
            
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task<IEnumerable<Alert>> GetCurrentAlerts()
    {
        var response = await _httpClient.GetAsync(CurrentAlertsUrl);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<IEnumerable<Alert>>();

        return data ?? [];
    }

    private async Task<IEnumerable<HistoricalAlert>> GetHistoricalAlerts()
    {
        var response = await _httpClient.GetAsync(AlertsHistoryUrl);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<IEnumerable<HistoricalAlert>>(_serializerOptions);

        return data ?? [];
    }

    private void OnAlertReceived(Alert alert)
    {
        AlertReceived?.Invoke(this, new AlertEventArgs
        {
            Alert = alert
        });
    }
}