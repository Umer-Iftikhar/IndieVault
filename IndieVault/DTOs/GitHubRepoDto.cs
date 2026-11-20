using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    public class GitHubRepoDto
    {
        [JsonPropertyName("stargazers_count")]
        public int StargazerCount { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }
}
