using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Repositories.Implementations
{
    public class GameRepository : Repository<Game>, IGameRepository
    {
        public GameRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<List<Game>> GetGamesByDevIdAsync(string devId)
        {
            var result = await _context.Games.Where(g => g.DeveloperId == devId)
                .Include(g => g.Genre)
                .Include(g => g.Engine)
                .ToListAsync();

            return result;
        }
        public async Task<Game?> GetGameIfOwnerAsync(int gameId, string userId)
        {
            var game = await _context.Games
                .Where(g => g.Id == gameId && g.DeveloperId == userId)
                .FirstOrDefaultAsync();
            return game;
        }
        public async Task<Game?> GetGameWithPlatformsAndTagsAsync(int gameId)
        {
            var game = await _context.Games
                .Where(g => g.Id == gameId)
                .Include(g => g.GamePlatforms)
                    .ThenInclude(gp => gp.Platform)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .FirstOrDefaultAsync();
            return game;
        }
        public async Task<Game?> GetGameWithDetailsAsync(int gameId)
        {
            var game = await _context.Games
               .Include(g => g.Developer)
               .Include(g => g.Genre)
               .Include(g => g.Engine)
               .Include(g => g.GamePlatforms)
                   .ThenInclude(gp => gp.Platform)
               .Include(g => g.GameTags)
                   .ThenInclude(gt => gt.Tag)
               .Include(g => g.Screenshots)
               .Include(g => g.Reviews)
                   .ThenInclude(r => r.User)
               .FirstOrDefaultAsync(g => g.Id == gameId);
            return game;
        }
        public async Task<int> GetGameCountByDevIdAsync(string devId)
        {
            return await _context.Games.CountAsync(g => g.DeveloperId == devId);
        }
        public async Task<int> CountGamesAsync()
        {
            return await _context.Games.CountAsync();
        }
        public async Task<bool> GameExistsByGenreIdAsync(int genreId)
        {
            return await _context.Games.AnyAsync(g => g.GenreId == genreId);
        }
        public async Task<bool> GameExistsByTitleOrExternalIdAsync(string title, int externalId)
        {
            return await _context.Games.AnyAsync(g => g.Title == title || g.ExternalApiId == externalId);
        }
    }
}
