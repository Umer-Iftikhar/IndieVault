using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    // Request body for the Gemini Interactions API (POST /v1beta/interactions).
    public class GeminiInteractionRequestDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;

        [JsonPropertyName("response_format")]
        public GeminiResponseFormatDto ResponseFormat { get; set; } = new();
    }
}
