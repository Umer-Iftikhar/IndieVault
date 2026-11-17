using IndieVault.DTOs;
using IndieVault.Enums;

namespace IndieVault.Repositories.Interfaces
{
    public interface IGameBrowseRepository
    {
        Task<IEnumerable<FeaturedGameDto>> GetFeaturedGamesAsync();
        Task<(List<GameBrowseDto> Games, int TotalCount)> GetPagedGamesAsync(int pageNumber, int pageSize, string? searchTerm, 
            decimal? minPrice, decimal? maxPrice, int? genreId, List<int>? platformIds, SortBy sortBy);
        Task<List<LookupDto>> GetGenreLookupsAsync();
        Task<List<LookupDto>> GetPlatformLookupsAsync();
    }
}
