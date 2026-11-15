using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Moq;

namespace IndieVault.Tests.Services
{
    public class GameServiceDetailsTests
    {
        private static Game FullGame() => new()
        {
            Id = 3,
            Title = "Hollow Knight",
            Description = "Bugs and nails.",
            Price = 14.99m,
            ReleaseDate = new DateTime(2017, 2, 24),
            DeveloperId = "dev-1",
            Developer = new ApplicationUser { Id = "dev-1", UserName = "teamcherry" },
            Genre = new Genre { Name = "Action" },
            Engine = new Engine { Name = "Unity" },
            CoverImagePath = "/images/games/3/cover.png",
            DownloadLink = "https://example.com/hk",
            GamePlatforms = new List<GamePlatform>
            {
                new() { Platform = new Platform { Name = "Windows" } },
                new() { Platform = new Platform { Name = "Linux" } }
            },
            GameTags = new List<GameTag> { new() { Tag = new Tag { Name = "2D" } } },
            Screenshots = new List<Screenshot> { new() { ImagePath = "/images/games/3/screen_1.png" } },
            Reviews = new List<Review>
            {
                new() { Id = 10, UserId = "player-1", User = new ApplicationUser { UserName = "playerone" }, Rating = 5, Comment = "Amazing" }
            }
        };

        [Fact]
        public async Task GetGameDetailsAsync_MissingGame_ThrowsArgumentException()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithDetailsAsync(3)).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => builder.Build().GetGameDetailsAsync(3, "player-1"));
        }

        [Fact]
        public async Task GetGameDetailsAsync_MapsGameAndRelatedData()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithDetailsAsync(3)).ReturnsAsync(FullGame());

            var dto = await builder.Build().GetGameDetailsAsync(3, "player-1");

            Assert.Equal(3, dto.GameId);
            Assert.Equal("teamcherry", dto.DeveloperName);
            Assert.Equal("dev-1", dto.DeveloperId);
            Assert.Equal("Action", dto.GenreName);
            Assert.Equal("Unity", dto.EngineName);
            Assert.Equal(new[] { "Windows", "Linux" }, dto.PlatformNames);
            Assert.Equal(new[] { "2D" }, dto.Tags);
            Assert.Equal("/images/games/3/screen_1.png", Assert.Single(dto.Screenshots).ImagePath);
            var review = Assert.Single(dto.Reviews);
            Assert.Equal("playerone", review.ReviewerName);
            Assert.Equal(5, review.Rating);
            Assert.Equal(10, review.Id);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task GetGameDetailsAsync_SetsWishlistAndReviewFlagsForCurrentUser(bool wishlisted, bool reviewed)
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithDetailsAsync(3)).ReturnsAsync(FullGame());
            builder.Wishlists.Setup(w => w.WishlistExistsAsync("player-1", 3)).ReturnsAsync(wishlisted);
            builder.Reviews.Setup(r => r.ReviewExistsAsync("player-1", 3)).ReturnsAsync(reviewed);

            var dto = await builder.Build().GetGameDetailsAsync(3, "player-1");

            Assert.Equal(wishlisted, dto.HasWishlisted);
            Assert.Equal(reviewed, dto.HasReviewed);
        }
    }
}
