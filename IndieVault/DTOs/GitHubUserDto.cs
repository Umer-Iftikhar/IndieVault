using System.Text.Json.Serialization;

namespace IndieVault.DTOs
{
    public class GitHubUserDto
    {
        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("public_repos")]
        public int PublicRepos { get; set; }
    }
}
