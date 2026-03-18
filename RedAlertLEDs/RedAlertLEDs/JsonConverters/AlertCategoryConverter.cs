using System.Text.Json;
using System.Text.Json.Serialization;
using RedAlertLEDs.BO;

namespace RedAlertLEDs.JsonConverters;

public class AlertCategoryConverter : JsonConverter<AlertCategory>
{
    public override AlertCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();

            if (int.TryParse(str, out var value))
            {
                return (AlertCategory)value;
            }

            if (Enum.TryParse<AlertCategory>(str, true, out var enumValue))
            {
                return enumValue;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return (AlertCategory)reader.GetInt32();
        }

        throw new JsonException($"Cannot convert to {nameof(AlertCategory)}");
    }

    public override void Write(Utf8JsonWriter writer, AlertCategory value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}