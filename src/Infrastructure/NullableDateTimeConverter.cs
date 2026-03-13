using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreCodeCamp.Infrastructure;

/// <summary>
/// Custom JSON converter for DateTime? that gracefully handles invalid/empty values by returning null
/// instead of throwing a deserialization error. This allows FluentValidation to report the error clearly.
/// </summary>
public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.String => string.IsNullOrWhiteSpace(reader.GetString()) 
                    ? null 
                    : DateTime.Parse(reader.GetString() ?? string.Empty),
                _ => null
            };
        }
        catch
        {
            // Return null for any parsing errors; let FluentValidation report the issue
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
