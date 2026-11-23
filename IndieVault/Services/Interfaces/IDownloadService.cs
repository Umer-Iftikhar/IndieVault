using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces
{
    public interface IDownloadService
    {
        Task<string> DownloadGameAsync(int gameId, string userId);
        Task<List<DownloadHistoryDto>> GetDownloadHistoryAsync(string userId);
    }
}
