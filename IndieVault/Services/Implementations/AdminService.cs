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
        public async Task AdminCreateGenreAsync(string genreName)
        {
            _logger.LogInformation("Admin creating genre: {GenreName}", genreName);
            // Check if the genre already exists before creating a new one
            bool existingGenre = await GenreExistsAsync(genreName);
            if (existingGenre)
            {
                _logger.LogWarning("Genre already exists: {GenreName}", genreName);
                throw new InvalidOperationException("Genre already exists.");
            }
            // Create and save the new genre
            var genre = new Genre
            {
                Name = genreName
            };
            // Save the new genre to the database
            await _genreRepository.CreateAsync(genre);
            _logger.LogInformation("Genre created: {GenreName} with Id {GenreId}", genreName, genre.Id);
        }
        public async Task<bool> GenreExistsAsync(string genreName)
        {
            _logger.LogInformation("Checking if genre exists: {GenreName}", genreName);
            return await _genreRepository.GenreExistsByNameAsync(genreName);
        }
        public async Task AdminDeleteGenreAsync(int genreId)
        {
            _logger.LogInformation("Admin deleting genre: {GenreId}", genreId);
            // Check if the genre exists before attempting to delete it
            var genre = await _genreRepository.GetByIdAsync(genreId);
            if (genre == null)
            {
                _logger.LogWarning("Genre {GenreId} not found for deletion", genreId);
                throw new KeyNotFoundException("Genre not found.");
            }
            // Check if there are any games associated with the genre before deleting it
            if (await _gameRepository.GameExistsByGenreIdAsync(genreId))
            {
                _logger.LogWarning("Cannot delete genre {GenreId} because it has associated games", genreId);
                throw new InvalidOperationException("Cannot delete genre because it is associated with existing games.");
            }
            // If the genre exists and has no associated games, proceed to delete it
            await _genreRepository.DeleteAsync(genreId);
            _logger.LogInformation("Genre {GenreId} deleted successfully", genreId);
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
        public async Task<LookupDto> GetGenreByIdAsync(int genreId)
        {
            _logger.LogInformation("Fetching genre by Id {GenreId}", genreId);

            // Check if the genre exists before attempting to retrieve it
            var genre = await _genreRepository.GetByIdAsync(genreId);
            if (genre == null)
            {
                _logger.LogWarning("Genre {GenreId} not found", genreId);
                throw new KeyNotFoundException("Genre not found.");
            }
            // If the genre exists, return a LookupDto containing its ID and name
            return new LookupDto
            {
                Id = genre.Id,
                Name = genre.Name
            };
        }
    }
}
