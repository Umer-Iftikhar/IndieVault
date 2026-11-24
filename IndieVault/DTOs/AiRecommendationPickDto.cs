using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    public class AiRecommendationPickDto
    {
        [JsonPropertyName("gameId")]
        public int GameId { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}
