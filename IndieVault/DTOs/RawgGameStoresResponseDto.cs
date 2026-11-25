using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    public class RawgGameStoresResponseDto
    {
        [JsonPropertyName("results")]
        public List<RawgGameStoreDto> Results { get; set; } = new();
    }
}
