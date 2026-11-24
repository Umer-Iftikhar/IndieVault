using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Repositories
{
    public class RecommendationRepositoryTests
    {
        [Fact]
        public async Task GetGamesUserLikedAsync_ReturnsWishlistedDownloadedAndHighlyRatedGames()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var wishlisted = data.AddGame("Wishlisted");
            var downloaded = data.AddGame("Downloaded");
            var lovedIt = data.AddGame("Rated 5");
            var hatedIt = data.AddGame("Rated 2");
            data.AddGame("Never touched");
            context.Wishlists.Add(new Wishlist { GameId = wishlisted.Id, UserId = data.Player.Id });
            context.DownloadHistories.Add(new DownloadHistory { GameId = downloaded.Id, UserId = data.Player.Id });
            context.Reviews.AddRange(
                new Review { GameId = lovedIt.Id, UserId = data.Player.Id, Rating = 5 },
                new Review { GameId = hatedIt.Id, UserId = data.Player.Id, Rating = 2 });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            var liked = await new RecommendationRepository(context).GetGamesUserLikedAsync(data.Player.Id);

            Assert.Equal(new[] { "Downloaded", "Rated 5", "Wishlisted" }, liked.Select(g => g.Title).OrderBy(t => t));
        }

        [Fact]
        public async Task GetGamesUserLikedAsync_LoadsGenreAndTags()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            var tag = new Tag { Name = "Pixel Art" };
            context.Tags.Add(tag);
            context.SaveChanges();
            context.GameTags.Add(new GameTag { GameId = game.Id, TagId = tag.Id });
            context.Wishlists.Add(new Wishlist { GameId = game.Id, UserId = data.Player.Id });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            var liked = await new RecommendationRepository(context).GetGamesUserLikedAsync(data.Player.Id);

            var likedGame = Assert.Single(liked);
            Assert.Equal("Action", likedGame.Genre.Name);
            Assert.Equal("Pixel Art", Assert.Single(likedGame.GameTags).Tag.Name);
        }

        [Fact]
        public async Task GetGamesUserLikedAsync_SameGameWishlistedAndDownloaded_ReturnedOnce()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            context.Wishlists.Add(new Wishlist { GameId = game.Id, UserId = data.Player.Id });
            context.DownloadHistories.Add(new DownloadHistory { GameId = game.Id, UserId = data.Player.Id });
            context.SaveChanges();

            var liked = await new RecommendationRepository(context).GetGamesUserLikedAsync(data.Player.Id);

            Assert.Single(liked);
        }

        [Fact]
        public async Task GetGamesUserLikedAsync_NewPlayer_ReturnsEmpty()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            data.AddGame();

            Assert.Empty(await new RecommendationRepository(context).GetGamesUserLikedAsync(data.Player.Id));
        }

        [Fact]
        public async Task GetGamesUserLikedAsync_IgnoresOtherUsersActivity()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            context.Wishlists.Add(new Wishlist { GameId = game.Id, UserId = data.Developer.Id });
            context.SaveChanges();

            Assert.Empty(await new RecommendationRepository(context).GetGamesUserLikedAsync(data.Player.Id));
        }

        [Fact]
        public async Task GetKnownGameIdsAsync_IncludesLowRatedReviewsToo()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var wishlisted = data.AddGame("Wishlisted");
            var downloaded = data.AddGame("Downloaded");
            var hatedIt = data.AddGame("Rated 1");
            var untouched = data.AddGame("Never touched");
            context.Wishlists.Add(new Wishlist { GameId = wishlisted.Id, UserId = data.Player.Id });
            context.DownloadHistories.Add(new DownloadHistory { GameId = downloaded.Id, UserId = data.Player.Id });
            context.Reviews.Add(new Review { GameId = hatedIt.Id, UserId = data.Player.Id, Rating = 1 });
            context.SaveChanges();

            var known = await new RecommendationRepository(context).GetKnownGameIdsAsync(data.Player.Id);

            Assert.Equal(new[] { wishlisted.Id, downloaded.Id, hatedIt.Id }.OrderBy(id => id), known.OrderBy(id => id));
            Assert.DoesNotContain(untouched.Id, known);
        }

        [Fact]
        public async Task GetCandidateGamesAsync_ExcludesGivenGames()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var known = data.AddGame("Known");
            data.AddGame("New 1");
            data.AddGame("New 2");
            context.ChangeTracker.Clear();

            var candidates = await new RecommendationRepository(context)
                .GetCandidateGamesAsync(new List<int> { known.Id }, maxCount: 10);

            Assert.Equal(new[] { "New 1", "New 2" }, candidates.Select(c => c.Title).OrderBy(t => t));
        }

        [Fact]
        public async Task GetCandidateGamesAsync_MapsGenreDeveloperTagsAndAverageRating()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame("Celeste");
            var tag = new Tag { Name = "2D" };
            context.Tags.Add(tag);
            context.SaveChanges();
            context.GameTags.Add(new GameTag { GameId = game.Id, TagId = tag.Id });
            context.Reviews.AddRange(
                new Review { GameId = game.Id, UserId = data.Player.Id, Rating = 5 },
                new Review { GameId = game.Id, UserId = data.Developer.Id, Rating = 4 });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            var candidates = await new RecommendationRepository(context).GetCandidateGamesAsync(new List<int>(), maxCount: 10);

            var candidate = Assert.Single(candidates);
            Assert.Equal(game.Id, candidate.Id);
            Assert.Equal("Celeste", candidate.Title);
            Assert.Equal("Action", candidate.GenreName);
            Assert.Equal("devone", candidate.Developer);
            Assert.Equal(new[] { "2D" }, candidate.Tags);
            Assert.Equal(4.5, candidate.AverageRating);
            Assert.Equal(9.99m, candidate.Price);
        }

        [Fact]
        public async Task GetCandidateGamesAsync_GameWithoutReviews_HasZeroRating()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            data.AddGame();
            context.ChangeTracker.Clear();

            var candidates = await new RecommendationRepository(context).GetCandidateGamesAsync(new List<int>(), maxCount: 10);

            Assert.Equal(0.0, Assert.Single(candidates).AverageRating);
        }

        [Fact]
        public async Task GetCandidateGamesAsync_RespectsMaxCount()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            for (var i = 1; i <= 5; i++)
            {
                data.AddGame($"Game {i}");
            }
            context.ChangeTracker.Clear();

            var candidates = await new RecommendationRepository(context).GetCandidateGamesAsync(new List<int>(), maxCount: 3);

            Assert.Equal(3, candidates.Count);
        }
    }
}
