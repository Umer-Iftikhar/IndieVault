using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Repositories
{
    public class GameRepositoryTests
    {
        [Fact]
        public async Task GetGamesByDevIdAsync_ReturnsOnlyThatDevelopersGames_WithGenreAndEngine()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var otherDev = new ApplicationUser { Id = "dev-2", UserName = "devtwo" };
            context.Users.Add(otherDev);
            context.SaveChanges();
            data.AddGame("Mine 1");
            data.AddGame("Mine 2");
            data.AddGame("Theirs", developerId: otherDev.Id);
            var repository = new GameRepository(context);

            var games = await repository.GetGamesByDevIdAsync(data.Developer.Id);

            Assert.Equal(2, games.Count);
            Assert.All(games, g =>
            {
                Assert.Equal(data.Developer.Id, g.DeveloperId);
                Assert.NotNull(g.Genre);
                Assert.NotNull(g.Engine);
            });
        }

        [Fact]
        public async Task GetGameIfOwnerAsync_ReturnsGame_ForOwner()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            var repository = new GameRepository(context);

            Assert.NotNull(await repository.GetGameIfOwnerAsync(game.Id, data.Developer.Id));
        }

        [Fact]
        public async Task GetGameIfOwnerAsync_ReturnsNull_ForSomeoneElse()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            var repository = new GameRepository(context);

            Assert.Null(await repository.GetGameIfOwnerAsync(game.Id, data.Player.Id));
        }

        [Fact]
        public async Task GetGameWithPlatformsAndTagsAsync_LoadsJoinEntities()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            var platform = new Platform { Name = "Windows" };
            var tag = new Tag { Name = "Indie" };
            context.Platforms.Add(platform);
            context.Tags.Add(tag);
            context.SaveChanges();
            context.GamePlatforms.Add(new GamePlatform { GameId = game.Id, PlatformId = platform.Id });
            context.GameTags.Add(new GameTag { GameId = game.Id, TagId = tag.Id });
            context.SaveChanges();
            context.ChangeTracker.Clear();
            var repository = new GameRepository(context);

            var loaded = await repository.GetGameWithPlatformsAndTagsAsync(game.Id);

            Assert.NotNull(loaded);
            Assert.Equal("Windows", Assert.Single(loaded!.GamePlatforms).Platform.Name);
            Assert.Equal("Indie", Assert.Single(loaded.GameTags).Tag.Name);
        }

        [Fact]
        public async Task GetGameWithDetailsAsync_LoadsDeveloperScreenshotsAndReviewers()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            context.Screenshots.Add(new Screenshot { GameId = game.Id, ImagePath = "/images/games/1/screen_1.png" });
            context.Reviews.Add(new Review { GameId = game.Id, UserId = data.Player.Id, Rating = 5, Comment = "Great" });
            context.SaveChanges();
            context.ChangeTracker.Clear();
            var repository = new GameRepository(context);

            var loaded = await repository.GetGameWithDetailsAsync(game.Id);

            Assert.NotNull(loaded);
            Assert.Equal("devone", loaded!.Developer.UserName);
            Assert.Equal("Action", loaded.Genre.Name);
            Assert.Equal("Unity", loaded.Engine.Name);
            Assert.Single(loaded.Screenshots);
            Assert.Equal("playerone", Assert.Single(loaded.Reviews).User.UserName);
        }

        [Fact]
        public async Task GetGameWithDetailsAsync_ReturnsNull_WhenGameMissing()
        {
            using var context = InMemoryContext.Create();
            var repository = new GameRepository(context);

            Assert.Null(await repository.GetGameWithDetailsAsync(123));
        }

        [Fact]
        public async Task Counts_ReturnPerDeveloperAndTotal()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            data.AddGame("A");
            data.AddGame("B");
            var repository = new GameRepository(context);

            Assert.Equal(2, await repository.GetGameCountByDevIdAsync(data.Developer.Id));
            Assert.Equal(0, await repository.GetGameCountByDevIdAsync(data.Player.Id));
            Assert.Equal(2, await repository.CountGamesAsync());
        }

        [Fact]
        public async Task GameExistsByGenreIdAsync_TrueOnlyWhenGenreIsUsed()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var unused = new Genre { Name = "Sports" };
            context.Genres.Add(unused);
            context.SaveChanges();
            data.AddGame();
            var repository = new GameRepository(context);

            Assert.True(await repository.GameExistsByGenreIdAsync(data.Genre.Id));
            Assert.False(await repository.GameExistsByGenreIdAsync(unused.Id));
        }

        [Theory]
        [InlineData("Hades", 0, true)]       // same title
        [InlineData("Other title", 3498, true)] // same RAWG id
        [InlineData("Other title", 1, false)]
        public async Task GameExistsByTitleOrExternalIdAsync_DetectsDuplicates(string title, int externalId, bool expected)
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            data.AddGame("Hades", externalApiId: 3498);
            var repository = new GameRepository(context);

            Assert.Equal(expected, await repository.GameExistsByTitleOrExternalIdAsync(title, externalId));
        }
    }
}
