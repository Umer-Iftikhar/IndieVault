using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using Microsoft.AspNetCore.Identity;

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
        public RawgApiService(HttpClient httpclient, ILogger<RawgApiService> logger, IConfiguration configuration, 
            IGameService gameService, IGameRepository gameRepository, IGenreRepository genreRepository, 
            IEngineRepository engineRepository, UserManager<ApplicationUser> userManager)
        {
            _httpclient = httpclient;
            _logger = logger;
            _configuration = configuration;
            _gameService = gameService;
            _gameRepository = gameRepository;
            _genreRepository = genreRepository;
            _engineRepository = engineRepository;
            _userManager = userManager;
        }
        private string GetApiKey()
        {
            return _configuration["RawgApi:Key"] ?? throw new InvalidOperationException("RAWG API key is not configured.");
        }

        // <summary>
        // Maps a RawgGameDto to a RawgGameUploadDto for creating a new game in the system.
        // This method extracts necessary information from the RawgGameDto and fills in the required fields for creating a game, including handling genres, engines, and assigning a developer.
        // </summary>
        private async Task<RawgGameUploadDto> MapRawgGameToCreateDtoAsync(RawgGameDto rawgGameDto)
        {

            var genreName = rawgGameDto.Genres.FirstOrDefault()?.Name; // Get the name of the first genre, if available

            // Try to find the genre by name, if not found, fallback to "Uncategorized". If "Uncategorized" is also not found, throw an exception.
            var genre = (genreName != null ? await _genreRepository.GetGenreByNameAsync(genreName) : null)
            ?? await _genreRepository.GetGenreByNameAsync("Uncategorized")
            ?? throw new InvalidOperationException("Uncategorized genre not found.");

            var engine = await _engineRepository.GetEngineByNameAsync("Unknown"); // Since RAWG doesn't provide engine information, we can assign a default "Unknown" engine. 

            var admin = await _userManager.GetUsersInRoleAsync("Admin"); // Assign "Admin" user as developer for imported games. 

            var domain = rawgGameDto.Stores.FirstOrDefault()?.Store?.Domain;

            return new RawgGameUploadDto
            {
                Title = rawgGameDto.Title,
                DeveloperId = admin.FirstOrDefault()?.Id ?? throw new InvalidOperationException("No admin user found to assign as developer for the imported game."),
                ReleaseDate = rawgGameDto.ReleaseDate,
                Price = 0,
                DownloadLink = domain != null ? $"https://{domain}" : null,
                CoverImagePath = rawgGameDto.CoverImage,
                ExternalApiId = rawgGameDto.ExternalApiId,
                ExternalApiSource = "RAWG",
                IsFromExternalApi = true,
                Description = "Imported from RAWG API. No description available. ",
                EngineId = engine!.Id,
                GenreId = genre?.Id ?? throw new InvalidOperationException("No genre found for the imported game.")
            };
        }

        public async Task<IEnumerable<RawgGameDto>> FetchGamesFromApiAsync(int count)
        {
            // Fetch games from RAWG API
            var apiKey = GetApiKey();

            // RAWG API endpoint for fetching games with pagination and ordering by rating
            var response = await _httpclient.GetAsync($"games?key={apiKey}&page_size={count}&ordering=-rating");

            // Check if the response is successful
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch games from RAWG API. Status Code: {StatusCode}", response.StatusCode);
                // Return an empty list if the API call fails
                return Enumerable.Empty<RawgGameDto>();
            }

            // Read and deserialize the response content into the RawgApiResponseDto
            var result = await response.Content.ReadFromJsonAsync<RawgApiResponseDto>();

            // Return the list of games from the API response, or an empty list if the result is null
            return result?.Results ?? Enumerable.Empty<RawgGameDto>();
        }

        public async Task<int> SynchronizeGamesFromApiAsync()
        {
            try
            {
                _logger.LogInformation("Starting synchronization of games from RAWG API.");
                var apiGames = await FetchGamesFromApiAsync(30);
                int syncedCount = 0;

                foreach (var apiGame in apiGames)
                {
                    var existingGame = await _gameRepository.GameExistsByTitleOrExternalIdAsync(apiGame.Title, apiGame.ExternalApiId);
                    if (!existingGame)
                    {
                        var dto = await MapRawgGameToCreateDtoAsync(apiGame);
                        var result = await _gameService.CreateGameFromApiAsync(dto);
                        if (result) 
                        { 
                            syncedCount++;
                        }
                    }
                }
                _logger.LogInformation("Completed synchronization of games from RAWG API. Total games synchronized: {SyncedCount}", syncedCount);
                
                return syncedCount;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during RAWG API sync");
                return 0; // Return 0 to indicate that no games were synchronized due to the error)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during RAWG API sync");
                return 0;
            }
        }

    }
}
