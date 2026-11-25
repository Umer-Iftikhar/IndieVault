using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces
{
    public interface IDownloadService
    {
        // Records the download and returns the link to send the user to.
        // Throws InvalidOperationException when the game has no usable download link.
        Task<string> DownloadGameAsync(int gameId, string userId, CancellationToken cancellationToken = default);
        Task<List<DownloadHistoryDto>> GetDownloadHistoryAsync(string userId, CancellationToken cancellationToken = default);
    }
}
