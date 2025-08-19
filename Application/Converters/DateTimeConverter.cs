using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Converters
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string dateString = reader.GetString();
                if (DateTime.TryParse(dateString, out DateTime result))
                {
                    return result;
                }
            }
            
            // Si no se puede parsear, usar DateTime.MinValue como fallback
            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Formato mas simple: YYYY-MM-DD HH:MM
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm"));
        }
    }
}
