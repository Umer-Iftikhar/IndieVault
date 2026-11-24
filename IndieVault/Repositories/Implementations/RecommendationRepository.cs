using IndieVault.Data;
using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Repositories.Implementations
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private const int LikedRatingThreshold = 4;

        private readonly AppDbContext _context;

        public RecommendationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Game>> GetGamesUserLikedAsync(string userId, CancellationToken cancellationToken = default)
        {
            var wishlisted = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.GameId)
                .ToListAsync(cancellationToken);

            var downloaded = await _context.DownloadHistories
                .Where(d => d.UserId == userId)
                .Select(d => d.GameId)
                .ToListAsync(cancellationToken);

            var highlyRated = await _context.Reviews
                .Where(r => r.UserId == userId && r.Rating >= LikedRatingThreshold)
                .Select(r => r.GameId)
                .ToListAsync(cancellationToken);

            var likedIds = wishlisted.Concat(downloaded).Concat(highlyRated).Distinct().ToList();
            if (likedIds.Count == 0)
            {
                return new List<Game>();
            }

            return await _context.Games
                .AsNoTracking()
                .Where(g => likedIds.Contains(g.Id))
                .Include(g => g.Genre)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<int>> GetKnownGameIdsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var wishlisted = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.GameId)
                .ToListAsync(cancellationToken);

            var downloaded = await _context.DownloadHistories
                .Where(d => d.UserId == userId)
                .Select(d => d.GameId)
                .ToListAsync(cancellationToken);

            var reviewed = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => r.GameId)
                .ToListAsync(cancellationToken);

            return wishlisted.Concat(downloaded).Concat(reviewed).Distinct().ToList();
        }

        public async Task<List<RecommendationCandidateDto>> GetCandidateGamesAsync(List<int> excludedGameIds, int maxCount, CancellationToken cancellationToken = default)
        {
            var games = await _context.Games
                .AsNoTracking()
                .Where(g => !excludedGameIds.Contains(g.Id))
                .OrderByDescending(g => g.CreatedDate)
                .ThenByDescending(g => g.Id)
                .Take(maxCount)
                .Include(g => g.Genre)
                .Include(g => g.Developer)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.Reviews)
                .ToListAsync(cancellationToken);

            return games.Select(g => new RecommendationCandidateDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Price = g.Price,
                ReleaseDate = g.ReleaseDate,
                CoverImagePath = g.CoverImagePath,
                Developer = g.Developer.UserName ?? string.Empty,
                GenreName = g.Genre.Name,
                Tags = g.GameTags.Select(gt => gt.Tag.Name).ToList(),
                AverageRating = g.Reviews.Count == 0 ? 0 : g.Reviews.Average(r => r.Rating)
            }).ToList();
        }
    }
}
