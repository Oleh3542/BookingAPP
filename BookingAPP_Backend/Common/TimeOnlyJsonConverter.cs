using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingAPP_Backend.Common;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string FormatWithSeconds = "HH:mm:ss";
    private const string FormatWithoutSeconds = "HH:mm";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (TimeOnly.TryParseExact(value, FormatWithSeconds, out var result)) return result;
        if (TimeOnly.TryParseExact(value, FormatWithoutSeconds, out result)) return result;
        if (TimeOnly.TryParse(value, out result)) return result;

        throw new JsonException($"Не вдалося розпізнати час '{value}'. Очікується формат HH:mm або HH:mm:ss.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(FormatWithSeconds));
    }
}