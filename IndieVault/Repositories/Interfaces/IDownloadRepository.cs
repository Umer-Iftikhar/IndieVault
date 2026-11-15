using IndieVault.Models;

namespace IndieVault.Repositories.Interfaces
{
    public interface IDownloadRepository : IRepository<DownloadHistory>
    {
        Task<List<DownloadHistory>> GetDownloadHistoryByUserIdAsync(string userId);
    }
}
