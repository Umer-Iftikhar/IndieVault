using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.TestHelpers
{
    // Holds every GameService dependency as a mock so tests only set up what they use.
    public class GameServiceBuilder
    {
        public Mock<IWebHostEnvironment> Environment { get; } = new();
        public Mock<IEngineRepository> Engines { get; } = new();
        public Mock<IGenreRepository> Genres { get; } = new();
        public Mock<IPlatformRepository> Platforms { get; } = new();
        public Mock<ITagRepository> Tags { get; } = new();
        public Mock<IGameRepository> Games { get; } = new();
        public Mock<IScreenshotRepository> Screenshots { get; } = new();
        public Mock<IWishlistRepository> Wishlists { get; } = new();
        public Mock<IReviewRepository> Reviews { get; } = new();

        public GameServiceBuilder WithWebRoot(string path)
        {
            Environment.Setup(e => e.WebRootPath).Returns(path);
            return this;
        }

        public GameService Build() => new(
            Environment.Object,
            IdentityMocks.UserManager().Object,
            Engines.Object,
            Genres.Object,
            Platforms.Object,
            Tags.Object,
            Games.Object,
            Screenshots.Object,
            Wishlists.Object,
            Reviews.Object,
            NullLogger<GameService>.Instance);

        // Makes CreateAsync behave like the database: assigns an id and returns the entity.
        public GameServiceBuilder AssignIdOnCreate(int id)
        {
            Games.Setup(g => g.CreateAsync(It.IsAny<Game>()))
                .ReturnsAsync((Game game) => { game.Id = id; return game; });
            return this;
        }
    }
}
