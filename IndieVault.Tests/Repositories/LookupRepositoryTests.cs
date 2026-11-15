using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Repositories
{
    public class LookupRepositoryTests
    {
        [Theory]
        [InlineData("Action")]
        [InlineData("action")]
        [InlineData("ACTION")]
        public async Task GenreExistsByNameAsync_IsCaseInsensitive(string lookup)
        {
            using var context = InMemoryContext.Create();
            context.Genres.Add(new Genre { Name = "Action" });
            await context.SaveChangesAsync();
            var repository = new GenreRepository(context);

            Assert.True(await repository.GenreExistsByNameAsync(lookup));
        }

        [Fact]
        public async Task GenreExistsByNameAsync_ReturnsFalse_ForUnknownGenre()
        {
            using var context = InMemoryContext.Create();
            context.Genres.Add(new Genre { Name = "Action" });
            await context.SaveChangesAsync();
            var repository = new GenreRepository(context);

            Assert.False(await repository.GenreExistsByNameAsync("Strategy"));
        }

        [Fact]
        public async Task GetGenreByNameAsync_ReturnsMatchingGenre()
        {
            using var context = InMemoryContext.Create();
            context.Genres.AddRange(new Genre { Name = "Action" }, new Genre { Name = "Uncategorized" });
            await context.SaveChangesAsync();
            var repository = new GenreRepository(context);

            var genre = await repository.GetGenreByNameAsync("Uncategorized");

            Assert.NotNull(genre);
            Assert.Equal("Uncategorized", genre!.Name);
        }

        [Fact]
        public async Task GetGenreByNameAsync_ReturnsNull_WhenMissing()
        {
            using var context = InMemoryContext.Create();
            var repository = new GenreRepository(context);

            Assert.Null(await repository.GetGenreByNameAsync("Uncategorized"));
        }

        [Fact]
        public async Task GetEngineByNameAsync_ReturnsMatchingEngine()
        {
            using var context = InMemoryContext.Create();
            context.Engines.AddRange(new Engine { Name = "Unity" }, new Engine { Name = "Unknown" });
            await context.SaveChangesAsync();
            var repository = new EngineRepository(context);

            var engine = await repository.GetEngineByNameAsync("Unknown");

            Assert.NotNull(engine);
            Assert.Equal("Unknown", engine!.Name);
        }

        [Fact]
        public async Task GetEngineByNameAsync_ReturnsNull_WhenMissing()
        {
            using var context = InMemoryContext.Create();
            var repository = new EngineRepository(context);

            Assert.Null(await repository.GetEngineByNameAsync("Godot"));
        }

        [Fact]
        public async Task PlatformAndTagRepositories_ReturnAllRows()
        {
            using var context = InMemoryContext.Create();
            context.Platforms.AddRange(new Platform { Name = "Windows" }, new Platform { Name = "Linux" });
            context.Tags.AddRange(new Tag { Name = "Pixel Art" }, new Tag { Name = "Roguelike" }, new Tag { Name = "Co-op" });
            await context.SaveChangesAsync();

            var platforms = await new PlatformRepository(context).GetAllAsync();
            var tags = await new TagRepository(context).GetAllAsync();

            Assert.Equal(2, platforms.Count);
            Assert.Equal(3, tags.Count);
        }
    }
}
