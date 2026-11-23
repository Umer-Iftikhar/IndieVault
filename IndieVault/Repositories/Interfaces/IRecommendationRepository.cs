using IndieVault.DTOs;
using IndieVault.Models;

namespace IndieVault.Repositories.Interfaces
{
    public interface IRecommendationRepository
    {
        // Games the player wishlisted, downloaded or rated 4 stars and above (with genre and tags)
        Task<List<Game>> GetGamesUserLikedAsync(string userId, CancellationToken cancellationToken = default);

        // Every game the player already wishlisted, downloaded or reviewed: these are never recommended
        Task<List<int>> GetKnownGameIdsAsync(string userId, CancellationToken cancellationToken = default);

        // The newest games that are not in the excluded list
        Task<List<RecommendationCandidateDto>> GetCandidateGamesAsync(List<int> excludedGameIds, int maxCount, CancellationToken cancellationToken = default);
    }
}
