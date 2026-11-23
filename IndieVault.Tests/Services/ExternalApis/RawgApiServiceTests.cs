using System.Net;
using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations.ExternalApis;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Services.ExternalApis
{
    public class RawgApiServiceTests
    {
        private const string GamesJson = """
            {
              "results": [
                {
                  "id": 3498, "name": "Grand Theft Auto V", "released": "2013-09-17",
                  "background_image": "https://media.rawg.io/gta.jpg",
                  "genres": [ { "id": 4, "name": "Action" } ],
                  "stores": [ { "id": 1, "store": { "id": 1, "name": "Steam", "domain": "store.steampowered.com" } } ]
                },
                {
                  "id": 4200, "name": "Portal 2", "released": null,
                  "background_image": "https://media.rawg.io/portal2.jpg",
                  "genres": [ { "id": 7, "name": "Puzzle Platformer" } ],
                  "stores": []
                }
              ]
            }
            """;

        private readonly StubHttpMessageHandler _http = new();
        private readonly Mock<IGameService> _gameService = new();
        private readonly Mock<IGameRepository> _games = new();
        private readonly Mock<IGenreRepository> _genres = new();
        private readonly Mock<IEngineRepository> _engines = new();
        private readonly Mock<UserManager<ApplicationUser>> _userManager = IdentityMocks.UserManager();

        public RawgApiServiceTests()
        {
            _genres.Setup(g => g.GetGenreByNameAsync("Action")).ReturnsAsync(new Genre { Id = 1, Name = "Action" });
            _genres.Setup(g => g.GetGenreByNameAsync("Uncategorized")).ReturnsAsync(new Genre { Id = 10, Name = "Uncategorized" });
            _engines.Setup(e => e.GetEngineByNameAsync("Unknown")).ReturnsAsync(new Engine { Id = 6, Name = "Unknown" });
            _userManager.Setup(u => u.GetUsersInRoleAsync("Admin"))
                .ReturnsAsync(new List<ApplicationUser> { new() { Id = "admin-1", UserName = "admin" } });
            _gameService.Setup(s => s.CreateGameFromApiAsync(It.IsAny<RawgGameUploadDto>())).ReturnsAsync(true);
        }

        private RawgApiService Service(string? apiKey = "test-key")
        {
            var settings = new Dictionary<string, string?>();
            if (apiKey != null)
            {
                settings["RawgApi:Key"] = apiKey;
            }
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var client = new HttpClient(_http) { BaseAddress = new Uri("https://api.rawg.io/api/") };

            return new RawgApiService(client, NullLogger<RawgApiService>.Instance, configuration,
                _gameService.Object, _games.Object, _genres.Object, _engines.Object, _userManager.Object);
        }

        [Fact]
        public async Task FetchGamesFromApiAsync_RequestsTopRatedGamesWithKey_AndParsesResults()
        {
            _http.Respond("/api/games", GamesJson);

            var games = (await Service().FetchGamesFromApiAsync(5)).ToList();

            Assert.Equal("/api/games?key=test-key&page_size=5&ordering=-rating", Assert.Single(_http.Requests).RequestUri!.PathAndQuery);
            Assert.Equal(2, games.Count);
            Assert.Equal(3498, games[0].ExternalApiId);
            Assert.Equal("Grand Theft Auto V", games[0].Title);
            Assert.Equal(new DateTime(2013, 9, 17), games[0].ReleaseDate);
            Assert.Equal("store.steampowered.com", games[0].Stores.Single().Store.Domain);
            Assert.Null(games[1].ReleaseDate);
        }

        [Fact]
        public async Task FetchGamesFromApiAsync_ApiError_ReturnsEmpty()
        {
            _http.Respond("/api/games", "{}", HttpStatusCode.Unauthorized);

            Assert.Empty(await Service().FetchGamesFromApiAsync(5));
        }

        [Fact]
        public async Task FetchGamesFromApiAsync_MissingApiKey_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => Service(apiKey: null).FetchGamesFromApiAsync(5));
        }

        [Fact]
        public async Task SynchronizeGamesFromApiAsync_ImportsNewGamesAndMapsFields()
        {
            _http.Respond("/api/games", GamesJson);
            var imported = new List<RawgGameUploadDto>();
            _gameService.Setup(s => s.CreateGameFromApiAsync(It.IsAny<RawgGameUploadDto>()))
                .Callback<RawgGameUploadDto>(imported.Add)
                .ReturnsAsync(true);

            var count = await Service().SynchronizeGamesFromApiAsync();

            Assert.Equal(2, count);
            var gta = imported.Single(d => d.ExternalApiId == 3498);
            Assert.Equal("Grand Theft Auto V", gta.Title);
            Assert.Equal(1, gta.GenreId);                       // matched by genre name
            Assert.Equal(6, gta.EngineId);                      // Unknown engine
            Assert.Equal("admin-1", gta.DeveloperId);           // imported games belong to the admin
            Assert.Equal(0, gta.Price);
            Assert.Equal("https://store.steampowered.com", gta.DownloadLink);
            Assert.Equal("https://media.rawg.io/gta.jpg", gta.CoverImagePath);
            Assert.Equal("RAWG", gta.ExternalApiSource);
            Assert.True(gta.IsFromExternalApi);

            var portal = imported.Single(d => d.ExternalApiId == 4200);
            Assert.Equal(10, portal.GenreId);                   // unknown genre -> Uncategorized
            Assert.Null(portal.DownloadLink);                   // no store -> no link
        }

        [Fact]
        public async Task SynchronizeGamesFromApiAsync_SkipsGamesAlreadyImported()
        {
            _http.Respond("/api/games", GamesJson);
            _games.Setup(g => g.GameExistsByTitleOrExternalIdAsync("Grand Theft Auto V", 3498)).ReturnsAsync(true);

            var count = await Service().SynchronizeGamesFromApiAsync();

            Assert.Equal(1, count);
            _gameService.Verify(s => s.CreateGameFromApiAsync(It.Is<RawgGameUploadDto>(d => d.ExternalApiId == 3498)), Times.Never);
        }

        [Fact]
        public async Task SynchronizeGamesFromApiAsync_CountsOnlySuccessfulImports()
        {
            _http.Respond("/api/games", GamesJson);
            _gameService.Setup(s => s.CreateGameFromApiAsync(It.Is<RawgGameUploadDto>(d => d.ExternalApiId == 4200))).ReturnsAsync(false);

            Assert.Equal(1, await Service().SynchronizeGamesFromApiAsync());
        }

        [Fact]
        public async Task SynchronizeGamesFromApiAsync_NoAdminUser_ReturnsZero()
        {
            _http.Respond("/api/games", GamesJson);
            _userManager.Setup(u => u.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<ApplicationUser>());

            Assert.Equal(0, await Service().SynchronizeGamesFromApiAsync());
            _gameService.Verify(s => s.CreateGameFromApiAsync(It.IsAny<RawgGameUploadDto>()), Times.Never);
        }

        [Fact]
        public async Task SynchronizeGamesFromApiAsync_ApiDown_ReturnsZero()
        {
            _http.Respond("/api/games", "{}", HttpStatusCode.ServiceUnavailable);

            Assert.Equal(0, await Service().SynchronizeGamesFromApiAsync());
        }
    }
}
