using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Moq;

namespace IndieVault.Tests.Services
{
    public class GameServiceImportTests
    {
        private static RawgGameUploadDto Dto(DateTime? released) => new()
        {
            Title = "Portal 2",
            Description = "Imported from RAWG API. No description available. ",
            Price = 0,
            ReleaseDate = released,
            CoverImagePath = "https://media.rawg.io/portal2.jpg",
            DownloadLink = null,
            GenreId = 10,
            EngineId = 6,
            DeveloperId = "admin-1",
            ExternalApiId = 4200
        };

        [Fact]
        public async Task CreateGameFromApiAsync_SavesImportedGameAndReturnsTrue()
        {
            var builder = new GameServiceBuilder();
            Game? saved = null;
            builder.Games.Setup(g => g.CreateAsync(It.IsAny<Game>())).Callback<Game>(g => saved = g).ReturnsAsync((Game g) => g);

            var ok = await builder.Build().CreateGameFromApiAsync(Dto(new DateTime(2011, 4, 18)));

            Assert.True(ok);
            Assert.Equal("Portal 2", saved!.Title);
            Assert.Equal(4200, saved.ExternalApiId);
            Assert.Equal("RAWG", saved.ExternalApiSource);
            Assert.True(saved.IsFromExternalApi);
            Assert.Equal("admin-1", saved.DeveloperId);
            Assert.Equal(new DateTime(2011, 4, 18), saved.ReleaseDate);
            Assert.Null(saved.DownloadLink);
        }

        [Fact]
        public async Task CreateGameFromApiAsync_NoReleaseDate_UsesToday()
        {
            var builder = new GameServiceBuilder();
            Game? saved = null;
            builder.Games.Setup(g => g.CreateAsync(It.IsAny<Game>())).Callback<Game>(g => saved = g).ReturnsAsync((Game g) => g);

            await builder.Build().CreateGameFromApiAsync(Dto(released: null));

            Assert.Equal(DateTime.UtcNow.Date, saved!.ReleaseDate.Date);
        }

        [Fact]
        public async Task CreateGameFromApiAsync_SaveFails_ReturnsFalseInsteadOfThrowing()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.CreateAsync(It.IsAny<Game>())).ThrowsAsync(new InvalidOperationException("duplicate"));

            Assert.False(await builder.Build().CreateGameFromApiAsync(Dto(null)));
        }
    }
}
