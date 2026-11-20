using System.Net.Http.Json;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces.ExternalApis;

namespace IndieVault.Services.Implementations.ExternalApis
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpclient; 
        private readonly ILogger<GitHubService> _logger;
        public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
        {
            _httpclient = httpClient;
            _logger = logger;
        }
        public async Task<GitHubProfileDto?> GetProfileAsync(string username) 
        {
            try
            {
                // Fetch user profile
                var address = $"users/{username}";

                // Log the API call for debugging
                _logger.LogInformation("Fetching GitHub profile for user: {Username}", username);

                // Make the API call and handle potential null response
                var result = await _httpclient.GetFromJsonAsync<GitHubUserDto>(address);

                if (result == null)
                {
                    return null;
                }

                // Fetch user repositories
                var repos = $"users/{username}/repos";

                // Log the API call for debugging
                _logger.LogInformation("Fetching GitHub repositories for user: {Username}", username);

                // Make the API call and handle potential null response
                List<GitHubRepoDto> repoResult = await _httpclient.GetFromJsonAsync<List<GitHubRepoDto>>(repos);

                if (repoResult == null) 
                {
                    return null;
                }

                // Process repositories to calculate total stars and top languages
                var totalStars = repoResult.Sum(repo => repo.StargazerCount);
                var topLanguages = repoResult 
                    .Where(r => r.Language != null)
                    .GroupBy(r => r.Language)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key);

                // Return the profile data as a DTO
                return new GitHubProfileDto
                {
                    ProfileUrl = result.HtmlUrl,
                    PublicRepos = result.PublicRepos,
                    TotalStars = totalStars,
                    TopLanguages = topLanguages.ToList(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GitHub API error for user: {Username}", username);
                return null;
            }
        }
    }
}
