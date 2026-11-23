using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    public class RawgStoreDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;
    }
}
