using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<RecommendationResultDto> GetRecommendationsAsync(string userId, CancellationToken cancellationToken = default);

        // Drops the cached result so the next request builds a fresh one
        void ClearRecommendations(string userId);
    }
}
