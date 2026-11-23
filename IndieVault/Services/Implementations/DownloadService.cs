using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Interfaces;

namespace IndieVault.Services.Implementations
{
    public class DownloadService : IDownloadService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IDownloadRepository _downloadRepository;
        private readonly ILogger<DownloadService> _logger;
        public DownloadService(IGameRepository gameRepository, IDownloadRepository downloadRepository, ILogger<DownloadService> logger)
        {
            _gameRepository = gameRepository;
            _downloadRepository = downloadRepository;
            _logger = logger;
        }
        public async Task<string> DownloadGameAsync(int gameId, string userId)
        {
            _logger.LogInformation("User {UserId} attempting to download game {GameId}", userId, gameId);

            // Check if the game exists
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                _logger.LogWarning("Download failed: Game {GameId} not found for user {UserId}", gameId, userId);
                throw new KeyNotFoundException("Game not found.");
            }

            // Record the download history
            var model = new DownloadHistory
            {
                DownloadDate = DateTime.UtcNow,
                GameId = game.Id,
                UserId = userId
            };

            // Save the download history to the database
            await _downloadRepository.CreateAsync(model);
            _logger.LogInformation("Download recorded for user {UserId}, game {GameId}. Returning download link.", userId, gameId);

            return game.DownloadLink;
        }
        public async Task<List<DownloadHistoryDto>> GetDownloadHistoryAsync(string userId)
        {
            _logger.LogInformation("Retrieving download history for user {UserId}", userId);

            // Retrieve the download history for the user
            var gameList = await _downloadRepository.GetDownloadHistoryByUserIdAsync(userId);

            // Map the download history to DTOs
            return gameList.Select(g => new DownloadHistoryDto
            {
                GameId = g.Game.Id,
                GameName = g.Game.Title,
                DownloadTime = g.DownloadDate
            }).ToList();

        }
    }
}
