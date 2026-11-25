using IndieVault.Caching;
using IndieVault.DTOs;
using IndieVault.Helpers;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace IndieVault.Services.Implementations.ExternalApis
{
    public class RawgApiService : IRawgApiService
    {
        private readonly HttpClient _httpclient;
        private readonly ILogger<RawgApiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGameService _gameService;
        private readonly IGameRepository _gameRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IEngineRepository _engineRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _memoryCache;

        public RawgApiService(HttpClient httpclient, ILogger<RawgApiService> logger, IConfiguration configuration,
            IGameService gameService, IGameRepository gameRepository, IGenreRepository genreRepository,
            IEngineRepository engineRepository, UserManager<ApplicationUser> userManager, IMemoryCache memoryCache)
        {
            _httpclient = httpclient;
            _logger = logger;
            _configuration = configuration;
            _gameService = gameService;
            _gameRepository = gameRepository;
            _genreRepository = genreRepository;
            _engineRepository = engineRepository;
            _userManager = userManager;
            _memoryCache = memoryCache;
        }

        private string GetApiKey()
        {
            return _configuration["RawgApi:Key"] ?? throw new InvalidOperationException("RAWG API key is not configured.");
        }

        // Maps a RAWG game to the DTO used to create a game in IndieVault: first genre (or Uncategorized),
        // the Unknown engine, the admin as developer and the game's store page as download link.
        private async Task<RawgGameUploadDto> MapRawgGameToCreateDtoAsync(RawgGameDto rawgGameDto, CancellationToken cancellationToken)
        {
            var genreName = rawgGameDto.Genres.FirstOrDefault()?.Name; // Get the name of the first genre, if available

            // Try to find the genre by name, if not found, fallback to "Uncategorized". If "Uncategorized" is also not found, throw an exception.
            var genre = (genreName != null ? await _genreRepository.GetGenreByNameAsync(genreName, cancellationToken) : null)
                ?? await _genreRepository.GetGenreByNameAsync("Uncategorized", cancellationToken)
                ?? throw new InvalidOperationException("Uncategorized genre not found.");

            // RAWG doesn't provide engine information, so imported games get the default "Unknown" engine
            var engine = await _engineRepository.GetEngineByNameAsync("Unknown", cancellationToken);

            var admin = await _userManager.GetUsersInRoleAsync("Admin"); // Imported games belong to the admin

            // The games list only has the store's home page. The nested stores endpoint has the game's own page.
            var storeUrl = await FetchGameStoreUrlAsync(rawgGameDto.ExternalApiId, cancellationToken);

            return new RawgGameUploadDto
            {
                Title = rawgGameDto.Title,
                DeveloperId = admin.FirstOrDefault()?.Id ?? throw new InvalidOperationException("No admin user found to assign as developer for the imported game."),
                ReleaseDate = rawgGameDto.ReleaseDate,
                Price = 0,
                DownloadLink = storeUrl,
                CoverImagePath = rawgGameDto.CoverImage,
                ExternalApiId = rawgGameDto.ExternalApiId,
                ExternalApiSource = "RAWG",
                IsFromExternalApi = true,
                Description = "Imported from RAWG API. No description available. ",
                EngineId = engine!.Id,
                GenreId = genre.Id
            };
        }

        public async Task<IEnumerable<RawgGameDto>> FetchGamesFromApiAsync(int count, CancellationToken cancellationToken = default)
        {
            var apiKey = GetApiKey();

            // RAWG API endpoint for fetching games with pagination and ordering by rating
            var response = await _httpclient.GetAsync($"games?key={apiKey}&page_size={count}&ordering=-rating", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch games from RAWG API. Status Code: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<RawgGameDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<RawgApiResponseDto>(cancellationToken);

            return result?.Results ?? Enumerable.Empty<RawgGameDto>();
        }

        public async Task<string?> FetchGameStoreUrlAsync(int externalApiId, CancellationToken cancellationToken = default)
        {
            var apiKey = GetApiKey();

            try
            {
                var response = await _httpclient.GetAsync($"games/{externalApiId}/stores?key={apiKey}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch stores for RAWG game {ExternalApiId}. Status Code: {StatusCode}", externalApiId, response.StatusCode);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<RawgGameStoresResponseDto>(cancellationToken);

                // First store that has a usable link
                return result?.Results
                    .Select(store => DownloadLinkHelper.Normalize(store.Url))
                    .FirstOrDefault(url => url != null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                // One game without a link must not stop the whole sync
                _logger.LogWarning(ex, "Error fetching stores for RAWG game {ExternalApiId}", externalApiId);
                return null;
            }
        }

        public async Task<RawgSyncResultDto> SynchronizeGamesFromApiAsync(CancellationToken cancellationToken = default)
        {
            var result = new RawgSyncResultDto();

            try
            {
                _logger.LogInformation("Starting synchronization of games from RAWG API.");
                var apiGames = await FetchGamesFromApiAsync(30, cancellationToken);

                foreach (var apiGame in apiGames)
                {
                    var importedGame = await _gameRepository.GetGameByExternalApiIdAsync(apiGame.ExternalApiId, cancellationToken);
                    if (importedGame != null)
                    {
                        // Imported before: only repair its download link if it is missing or just a store home page
                        if (await RepairDownloadLinkAsync(importedGame, cancellationToken))
                        {
                            result.LinksUpdated++;
                        }
                        continue;
                    }

                    var existingGame = await _gameRepository.GameExistsByTitleOrExternalIdAsync(apiGame.Title, apiGame.ExternalApiId, cancellationToken);
                    if (!existingGame)
                    {
                        var dto = await MapRawgGameToCreateDtoAsync(apiGame, cancellationToken);
                        if (await _gameService.CreateGameFromApiAsync(dto, cancellationToken))
                        {
                            result.Imported++;
                        }
                    }
                }

                _logger.LogInformation("Completed synchronization of games from RAWG API. Imported: {Imported}, links updated: {LinksUpdated}",
                    result.Imported, result.LinksUpdated);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during RAWG API sync");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during RAWG API sync");
            }

            return result;
        }

        private async Task<bool> RepairDownloadLinkAsync(Game game, CancellationToken cancellationToken)
        {
            if (game.ExternalApiId == null || DownloadLinkHelper.IsStorePage(game.DownloadLink))
            {
                return false;
            }

            var storeUrl = await FetchGameStoreUrlAsync(game.ExternalApiId.Value, cancellationToken);
            if (storeUrl == null || storeUrl == game.DownloadLink)
            {
                return false;
            }

            game.DownloadLink = storeUrl;
            await _gameRepository.UpdateAsync(game, cancellationToken);
            _memoryCache.Remove(CacheKeys.GameDetails(game.Id));
            _logger.LogInformation("Updated download link for game {GameId} from RAWG stores", game.Id);
            return true;
        }
    }
}
