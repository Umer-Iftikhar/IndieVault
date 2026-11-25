using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces.ExternalApis
{
    public interface IRawgApiService
    {
        Task<IEnumerable<RawgGameDto>> FetchGamesFromApiAsync(int count, CancellationToken cancellationToken = default);

        // Calls the nested endpoint games/{id}/stores and returns the game's store page, or null if it has none
        Task<string?> FetchGameStoreUrlAsync(int externalApiId, CancellationToken cancellationToken = default);

        Task<RawgSyncResultDto> SynchronizeGamesFromApiAsync(CancellationToken cancellationToken = default);
    }
}
