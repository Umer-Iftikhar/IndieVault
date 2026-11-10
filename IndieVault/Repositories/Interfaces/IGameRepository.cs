using IndieVault.Models;

namespace IndieVault.Repositories.Interfaces
{
    public interface IGameRepository : IRepository<Game>
    {
        Task<List<Game>> GetGamesByDevIdAsync(string devId);
        Task<Game?> GetGameIfOwnerAsync(int gameId, string userId);
        Task<Game?> GetGameWithPlatformsAndTagsAsync(int gameId);
        Task<Game?> GetGameWithDetailsAsync(int gameId);
        Task<int> GetGameCountByDevIdAsync(string devId);
        Task<bool> GameExistsByGenreIdAsync(int genreId);
        Task<int> CountGamesAsync();

        // Check for existing game by title or external ID
        Task<bool> GameExistsByTitleOrExternalIdAsync(string title, int externalId);
    }
}
