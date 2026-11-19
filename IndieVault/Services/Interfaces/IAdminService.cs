using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetAdminDashboardDataAsync();
        Task IsGameFeatureAsync(int gameId);
    }
}
