    using System.Text.Json.Serialization;

    namespace IndieVault.DTOs
    {
        public class RawgGameDto
        {
            [JsonPropertyName("id")]
            public int ExternalApiId { get; set; }

            [JsonPropertyName("name")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("released")]
            public DateTime? ReleaseDate { get; set; }

            [JsonPropertyName("background_image")]
            public string CoverImage { get; set; } = string.Empty;

            [JsonPropertyName("genres")]
            public List<LookupDto> Genres { get; set; } = new();

            [JsonPropertyName("stores")]    
            public List<RawgStoreWrapperDto> Stores { get; set; } = new();
        }
    }
