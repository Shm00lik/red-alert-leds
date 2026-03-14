using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RedAlertLEDs.BO;

namespace RedAlertLEDs.Services;

public class HomeFrontCommandService(HttpClient httpClient)
{
    private const string CurrentAlertsUrl = "https://www.oref.org.il/warningMessages/alert/Alerts.json";
    private const string AlertsHistoryUrl = "https://www.oref.org.il/warningMessages/alert/History/AlertsHistory.json";

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public async Task<IEnumerable<Alert>> GetCurrentAlerts()
    {
        var response = await httpClient.GetAsync(CurrentAlertsUrl);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<IEnumerable<Alert>>();

        return data ?? [];
    }

    public async Task<IEnumerable<HistoricalAlert>> GetHistoricalAlerts()
    {
        var response = await httpClient.GetAsync(AlertsHistoryUrl);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<IEnumerable<HistoricalAlert>>(_serializerOptions);

        return data ?? [];
    }
}