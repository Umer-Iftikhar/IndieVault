using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    // One entry of GET games/{id}/stores: where a game can be bought, with the link to its store page.
    public class RawgGameStoreDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("game_id")]
        public int GameId { get; set; }

        [JsonPropertyName("store_id")]
        public int StoreId { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
