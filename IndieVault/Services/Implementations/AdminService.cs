using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Interfaces;

namespace IndieVault.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IAdminStatisticsRepository _adminStatisticsRepository;
        private readonly ILogger<AdminService> _logger;
        public AdminService
                (IReviewRepository reviewRepository, IGameRepository gameRepository, IGenreRepository genreRepository,
                IAdminStatisticsRepository adminStatisticsRepository, ILogger<AdminService> logger)
        {
            _reviewRepository = reviewRepository;
            _gameRepository = gameRepository;
            _genreRepository = genreRepository;
            _adminStatisticsRepository = adminStatisticsRepository;
            _logger = logger;
        }
        public async Task<AdminDashboardDto> GetAdminDashboardDataAsync()
        {
            _logger.LogInformation("Fetching admin dashboard data");
            // Fetch the most wishlisted game and user roles in parallel
            var mostWishlistedGame = await _adminStatisticsRepository.MostWishlistedGameAsync();
            var userRoles = await _adminStatisticsRepository.GetUsersByRoleAsync();

            // Fetch genres and games in parallel
            var genres = await _genreRepository.GetAllAsync();
            var games = await _gameRepository.GetAllAsync();

            _logger.LogInformation("Admin Dashboard data fetched: TotalGenres: {GenreCount}, TotalGames: {GameCount}", genres.Count, games.Count);
            return new AdminDashboardDto
            {
                TotalGames = await _gameRepository.CountGamesAsync(), 
                TotalReviews = await _reviewRepository.CountReviewsAsync(),
                MostWishlistedGame = mostWishlistedGame,
                UsersByRole = userRoles.ToList(),
                Genres = genres.Select(g => new LookupDto { Id = g.Id, Name = g.Name }).ToList(),
                Games = games.Select(g => new AdminGameDto
                {
                    Id = g.Id,
                    Title = g.Title,
                    IsFeatured = g.IsFeatured
                }).ToList()
            };
        }
        public async Task IsGameFeatureAsync(int gameId)
        {
            _logger.LogInformation("Admin toggling featured status for game {GameId}", gameId);

            // Check if the game exists before attempting to update its featured status
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                _logger.LogWarning("Game {GameId} not found for featured toggle", gameId);
                throw new KeyNotFoundException("Game not found.");
            }
            // Toggle the featured status of the game
            if (game.IsFeatured)
            {
                game.IsFeatured = false;
            }
            else
            {
                game.IsFeatured = true;
            }
            // Update the game in the database with the new featured status
            await _gameRepository.UpdateAsync(game);
            _logger.LogInformation("Game {GameId} featured status changed to {IsFeatured}", gameId, game.IsFeatured);
        }
    }
}
