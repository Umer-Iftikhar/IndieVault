using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    // Tells Gemini to answer with JSON that follows the given schema.
    public class GeminiResponseFormatDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = "application/json";

        [JsonPropertyName("schema")]
        public object? Schema { get; set; }
    }
}
