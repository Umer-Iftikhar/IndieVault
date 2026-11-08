using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Repositories
{
    // Exercises the generic Repository<T> through a concrete entity (Genre).
    public class RepositoryTests
    {
        [Fact]
        public async Task CreateAsync_PersistsEntityAndAssignsId()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);

            var created = await repository.CreateAsync(new Genre { Name = "Action" });

            Assert.True(created.Id > 0);
            Assert.Single(context.Genres);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity_WhenItExists()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);
            var genre = await repository.CreateAsync(new Genre { Name = "RPG" });

            var found = await repository.GetByIdAsync(genre.Id);

            Assert.NotNull(found);
            Assert.Equal("RPG", found!.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenMissing()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);

            Assert.Null(await repository.GetByIdAsync(999));
        }

        [Fact]
        public async Task ExistsAsync_ReflectsWhetherEntityExists()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);
            var genre = await repository.CreateAsync(new Genre { Name = "Puzzle" });

            Assert.True(await repository.ExistsAsync(genre.Id));
            Assert.False(await repository.ExistsAsync(genre.Id + 100));
        }

        [Fact]
        public async Task UpdateAsync_SavesChanges()
        {
            var dbName = Guid.NewGuid().ToString();
            int id;
            using (var context = InMemoryContext.Create(dbName))
            {
                var repository = new Repository<Genre>(context);
                var genre = await repository.CreateAsync(new Genre { Name = "Racng" });
                genre.Name = "Racing";
                await repository.UpdateAsync(genre);
                id = genre.Id;
            }

            using var verify = InMemoryContext.Create(dbName);
            Assert.Equal("Racing", (await verify.Genres.FindAsync(id))!.Name);
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntity()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);
            var genre = await repository.CreateAsync(new Genre { Name = "Horror" });

            await repository.DeleteAsync(genre.Id);

            Assert.Empty(context.Genres);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenEntityMissing()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.DeleteAsync(42));
            Assert.Contains("Genre", ex.Message);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEveryEntity()
        {
            using var context = InMemoryContext.Create();
            var repository = new Repository<Genre>(context);
            await repository.CreateAsync(new Genre { Name = "Action" });
            await repository.CreateAsync(new Genre { Name = "Sports" });

            var all = await repository.GetAllAsync();

            Assert.Equal(2, all.Count);
        }
    }
}
