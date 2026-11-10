using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Repositories
{
    public class ActivityRepositoryTests
    {
        [Fact]
        public async Task ReviewExistsAsync_MatchesUserAndGame()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var reviewed = data.AddGame("Reviewed");
            var notReviewed = data.AddGame("Not reviewed");
            context.Reviews.Add(new Review { GameId = reviewed.Id, UserId = data.Player.Id, Rating = 4 });
            context.SaveChanges();
            var repository = new ReviewRepository(context);

            Assert.True(await repository.ReviewExistsAsync(data.Player.Id, reviewed.Id));
            Assert.False(await repository.ReviewExistsAsync(data.Player.Id, notReviewed.Id));
            Assert.False(await repository.ReviewExistsAsync(data.Developer.Id, reviewed.Id));
        }

        [Fact]
        public async Task CountReviewsAsync_CountsAllReviews()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            context.Reviews.AddRange(
                new Review { GameId = game.Id, UserId = data.Player.Id, Rating = 4 },
                new Review { GameId = game.Id, UserId = data.Developer.Id, Rating = 2 });
            context.SaveChanges();

            Assert.Equal(2, await new ReviewRepository(context).CountReviewsAsync());
        }

        [Fact]
        public async Task WishlistExistsAsync_And_GetWishlistEntryAsync_MatchUserAndGame()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            context.Wishlists.Add(new Wishlist { GameId = game.Id, UserId = data.Player.Id });
            context.SaveChanges();
            var repository = new WishlistRepository(context);

            Assert.True(await repository.WishlistExistsAsync(data.Player.Id, game.Id));
            Assert.False(await repository.WishlistExistsAsync(data.Developer.Id, game.Id));
            Assert.NotNull(await repository.GetWishlistEntryAsync(data.Player.Id, game.Id));
            Assert.Null(await repository.GetWishlistEntryAsync(data.Player.Id, game.Id + 1));
        }

        [Fact]
        public async Task GetWishlistByUserIdAsync_ReturnsUsersItems_WithGameAndGenre()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game1 = data.AddGame("One");
            var game2 = data.AddGame("Two");
            context.Wishlists.AddRange(
                new Wishlist { GameId = game1.Id, UserId = data.Player.Id },
                new Wishlist { GameId = game2.Id, UserId = data.Player.Id },
                new Wishlist { GameId = game1.Id, UserId = data.Developer.Id });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            var items = await new WishlistRepository(context).GetWishlistByUserIdAsync(data.Player.Id);

            Assert.Equal(2, items.Count);
            Assert.All(items, w => Assert.Equal("Action", w.Game.Genre.Name));
        }

        [Fact]
        public async Task GetDownloadHistoryByUserIdAsync_ReturnsUsersDownloads_WithGame()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame("Downloaded");
            context.DownloadHistories.AddRange(
                new DownloadHistory { GameId = game.Id, UserId = data.Player.Id },
                new DownloadHistory { GameId = game.Id, UserId = data.Developer.Id });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            var history = await new DownloadRepository(context).GetDownloadHistoryByUserIdAsync(data.Player.Id);

            var entry = Assert.Single(history);
            Assert.Equal("Downloaded", entry.Game.Title);
        }
    }
}
