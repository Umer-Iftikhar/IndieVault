using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Tests.Data
{
    public class DatabaseSeederTests
    {
        [Fact]
        public async Task SeedGenres_EmptyDatabase_AddsAllGenresIncludingUncategorized()
        {
            using var context = InMemoryContext.Create();

            var genres = await DatabaseSeeder.SeedGenres(context);

            Assert.Equal(10, genres.Count);
            Assert.Equal(10, await context.Genres.CountAsync());
            Assert.Contains(genres, g => g.Name == "Uncategorized");
        }

        [Fact]
        public async Task SeedGenres_ExistingDataWithoutUncategorized_AddsOnlyUncategorized()
        {
            using var context = InMemoryContext.Create();
            context.Genres.AddRange(new Genre { Name = "Action" }, new Genre { Name = "RPG" });
            await context.SaveChangesAsync();

            var genres = await DatabaseSeeder.SeedGenres(context);

            Assert.Equal(3, genres.Count);
            Assert.Equal(3, await context.Genres.CountAsync());
            Assert.Contains(genres, g => g.Name == "Uncategorized");
        }

        [Fact]
        public async Task SeedGenres_RunTwice_DoesNotDuplicate()
        {
            using var context = InMemoryContext.Create();

            await DatabaseSeeder.SeedGenres(context);
            await DatabaseSeeder.SeedGenres(context);

            Assert.Equal(10, await context.Genres.CountAsync());
        }

        [Fact]
        public async Task SeedEngines_EmptyDatabase_AddsAllEnginesIncludingUnknown()
        {
            using var context = InMemoryContext.Create();

            var engines = await DatabaseSeeder.SeedEngines(context);

            Assert.Equal(6, engines.Count);
            Assert.Contains(engines, e => e.Name == "Unknown");
        }

        [Fact]
        public async Task SeedEngines_ExistingDataWithoutUnknown_AddsOnlyUnknown()
        {
            using var context = InMemoryContext.Create();
            context.Engines.Add(new Engine { Name = "Godot" });
            await context.SaveChangesAsync();

            var engines = await DatabaseSeeder.SeedEngines(context);

            Assert.Equal(2, await context.Engines.CountAsync());
            Assert.Contains(engines, e => e.Name == "Unknown");
        }

        [Fact]
        public async Task SeedPlatformsAndTags_EmptyDatabase_AddDefaults()
        {
            using var context = InMemoryContext.Create();

            var platforms = await DatabaseSeeder.SeedPlatforms(context);
            var tags = await DatabaseSeeder.SeedTags(context);

            Assert.Equal(5, platforms.Count);
            Assert.Equal(7, tags.Count);
        }

        [Fact]
        public async Task SeedPlatforms_ExistingData_ReturnsExistingWithoutAdding()
        {
            using var context = InMemoryContext.Create();
            context.Platforms.Add(new Platform { Name = "Switch" });
            await context.SaveChangesAsync();

            var platforms = await DatabaseSeeder.SeedPlatforms(context);

            Assert.Equal("Switch", Assert.Single(platforms).Name);
        }

        [Fact]
        public async Task SeedGamesAsync_Creates50GamesUsingGivenLookupsAndDevelopers()
        {
            using var context = InMemoryContext.Create();
            var genres = await DatabaseSeeder.SeedGenres(context);
            var engines = await DatabaseSeeder.SeedEngines(context);
            var devIds = new List<string> { "dev-1", "dev-2" };

            var games = await DatabaseSeeder.SeedGamesAsync(context, devIds, genres, engines);

            Assert.Equal(50, games.Count);
            Assert.All(games, g =>
            {
                Assert.Contains(g.DeveloperId, devIds);
                Assert.Contains(genres, x => x.Id == g.GenreId);
                Assert.Contains(engines, x => x.Id == g.EngineId);
                Assert.InRange(g.Price, 5m, 60m);
            });
        }

        [Fact]
        public async Task SeedGamesAsync_GamesAlreadyExist_ReturnsExistingGames()
        {
            using var context = InMemoryContext.Create();
            var genres = await DatabaseSeeder.SeedGenres(context);
            var engines = await DatabaseSeeder.SeedEngines(context);
            await DatabaseSeeder.SeedGamesAsync(context, new List<string> { "dev-1" }, genres, engines);

            var second = await DatabaseSeeder.SeedGamesAsync(context, new List<string> { "dev-1" }, genres, engines);

            Assert.Equal(50, second.Count);
            Assert.Equal(50, await context.Games.CountAsync());
        }

        [Fact]
        public async Task SeedGameRelations_RespectLimitsAndUniqueness()
        {
            using var context = InMemoryContext.Create();
            var genres = await DatabaseSeeder.SeedGenres(context);
            var engines = await DatabaseSeeder.SeedEngines(context);
            var platforms = await DatabaseSeeder.SeedPlatforms(context);
            var players = Enumerable.Range(1, 6).Select(i => $"player-{i}").ToList();
            var games = await DatabaseSeeder.SeedGamesAsync(context, new List<string> { "dev-1" }, genres, engines);

            await DatabaseSeeder.SeedGamePlatformsAsync(context, games, platforms);
            await DatabaseSeeder.SeedReviewsAsync(context, games, players);
            await DatabaseSeeder.SeedScreenshotsAsync(context, games);

            foreach (var game in games)
            {
                var platformIds = await context.GamePlatforms.Where(gp => gp.GameId == game.Id).Select(gp => gp.PlatformId).ToListAsync();
                Assert.InRange(platformIds.Count, 1, 3);
                Assert.Equal(platformIds.Count, platformIds.Distinct().Count());

                var reviews = await context.Reviews.Where(r => r.GameId == game.Id).ToListAsync();
                Assert.InRange(reviews.Count, 1, 5);
                Assert.Equal(reviews.Count, reviews.Select(r => r.UserId).Distinct().Count()); // one review per player
                Assert.All(reviews, r => Assert.InRange(r.Rating, 1, 5));

                Assert.InRange(await context.Screenshots.CountAsync(s => s.GameId == game.Id), 1, 5);
            }
        }
    }
}
