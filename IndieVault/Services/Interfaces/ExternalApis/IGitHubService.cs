using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces.ExternalApis
{
    public interface IGitHubService
    {
        Task<GitHubProfileDto?> GetProfileAsync(string username);
    }
}
