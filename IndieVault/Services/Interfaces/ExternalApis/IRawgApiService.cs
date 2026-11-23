using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces.ExternalApis
{
    public interface IRawgApiService
    {
        Task<IEnumerable<RawgGameDto>> FetchGamesFromApiAsync(int count);
        Task<int> SynchronizeGamesFromApiAsync();
    }
}
