using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    // The JSON we ask the AI model to return.
    public class AiRecommendationResponseDto
    {
        [JsonPropertyName("recommendations")]
        public List<AiRecommendationPickDto> Recommendations { get; set; } = new();
    }
}
