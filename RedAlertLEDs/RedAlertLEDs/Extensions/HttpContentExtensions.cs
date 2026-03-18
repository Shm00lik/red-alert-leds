using System.Text.Json;
using System.Text.Json.Serialization;
using RedAlertLEDs.JsonConverters;

namespace RedAlertLEDs.Extensions;

public static class HttpContentExtensions
{
    private const char StartArrayChar = '[';

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new AlertCategoryConverter() }
    };

    public static async Task<IEnumerable<T>> ReadOneOrMoreAsync<T>(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var isArray = json.StartsWith(StartArrayChar);

        if (isArray)
        {
            return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? [];
        }

        return [JsonSerializer.Deserialize<T>(json, SerializerOptions)!];
    }
}