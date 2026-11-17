using IndieVault.DTOs;
using IndieVault.Enums;
using IndieVault.Services.Interfaces;
using IndieVault.Repositories.Interfaces;

namespace IndieVault.Services.Implementations
{
    public class GameBrowseService : IGameBrowseService
    {
        private readonly IGameBrowseRepository _gameBrowseRepository;
        public GameBrowseService(IGameBrowseRepository gameBrowseRepository)
        {
           _gameBrowseRepository = gameBrowseRepository;
        }
        public async Task<(List<GameBrowseDto> Games, int TotalCount)> GetBrowseGamesAsync(int pageNumber, int pageSize, string? searchTerm, decimal? minPrice, decimal? maxPrice, int? genreId, List<int>? platformIds, SortBy sortBy)
        {
            var result = await _gameBrowseRepository.GetPagedGamesAsync(pageNumber, pageSize, searchTerm, minPrice, maxPrice, genreId, platformIds, sortBy); // Call repository method to get paged games
            return result;
        }
        public async Task<IEnumerable<FeaturedGameDto>> GetFeaturedGamesAsync()
        {
            // Call repository method to get featured games
            var result = await _gameBrowseRepository.GetFeaturedGamesAsync();
            return result;
        }

        public async Task<List<LookupDto>> GetGenreListAsync()
        {
            // Call repository method to get genre lookups
            var result = await _gameBrowseRepository.GetGenreLookupsAsync();
            return result;
        }

        public async Task<List<LookupDto>> GetPlatformListAsync()
        {
            // Call repository method to get platform lookups
            var result = await _gameBrowseRepository.GetPlatformLookupsAsync();
            return result;
        }
    }
}
