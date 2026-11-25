using IndieVault.DTOs;
using IndieVault.Helpers;
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

        public async Task<string> DownloadGameAsync(int gameId, string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} attempting to download game {GameId}", userId, gameId);

            var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
            if (game == null)
            {
                _logger.LogWarning("Download failed: Game {GameId} not found for user {UserId}", gameId, userId);
                throw new KeyNotFoundException("Game not found.");
            }

            // Check the link before recording anything: a download without a working link is not a download
            var downloadLink = DownloadLinkHelper.Normalize(game.DownloadLink);
            if (downloadLink == null)
            {
                _logger.LogWarning("Download failed: Game {GameId} has no usable download link", gameId);
                throw new InvalidOperationException("This game does not have a working download link yet.");
            }

            var model = new DownloadHistory
            {
                DownloadDate = DateTime.UtcNow,
                GameId = game.Id,
                UserId = userId
            };

            await _downloadRepository.CreateAsync(model, cancellationToken);
            _logger.LogInformation("Download recorded for user {UserId}, game {GameId}. Returning download link.", userId, gameId);

            return downloadLink;
        }

        public async Task<List<DownloadHistoryDto>> GetDownloadHistoryAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving download history for user {UserId}", userId);

            var gameList = await _downloadRepository.GetDownloadHistoryByUserIdAsync(userId, cancellationToken);

            return gameList.Select(g => new DownloadHistoryDto
            {
                GameId = g.Game.Id,
                GameName = g.Game.Title,
                DownloadTime = g.DownloadDate
            }).ToList();
        }
    }
}
