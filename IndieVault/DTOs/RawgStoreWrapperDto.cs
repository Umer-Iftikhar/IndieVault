using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    public class RawgStoreWrapperDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("store")]
        public RawgStoreDto Store { get; set; } = new();
    }
}
