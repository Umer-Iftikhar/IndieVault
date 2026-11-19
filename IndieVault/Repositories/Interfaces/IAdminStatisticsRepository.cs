using IndieVault.DTOs;

namespace IndieVault.Repositories.Interfaces
{
    public interface IAdminStatisticsRepository
    {
        Task<MostWishlistedGameDto?> MostWishlistedGameAsync();
        Task<List<UserByRoleDto>> GetUsersByRoleAsync();
    }
}
