using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Moq;

namespace IndieVault.Tests.Services
{
    public class AdminServiceGenreTests
    {
        [Fact]
        public async Task AdminCreateGenreAsync_DuplicateName_ThrowsAndDoesNotCreate()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GenreExistsByNameAsync("Action")).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => builder.Build().AdminCreateGenreAsync("Action"));

            Assert.Equal("Genre already exists.", ex.Message);
            builder.Genres.Verify(g => g.CreateAsync(It.IsAny<Genre>()), Times.Never);
        }

        [Fact]
        public async Task AdminCreateGenreAsync_NewName_CreatesGenre()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GenreExistsByNameAsync("Metroidvania")).ReturnsAsync(false);

            await builder.Build().AdminCreateGenreAsync("Metroidvania");

            builder.Genres.Verify(g => g.CreateAsync(It.Is<Genre>(x => x.Name == "Metroidvania")), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GenreExistsAsync_ReturnsRepositoryAnswer(bool exists)
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GenreExistsByNameAsync("Action")).ReturnsAsync(exists);

            Assert.Equal(exists, await builder.Build().GenreExistsAsync("Action"));
        }

        [Fact]
        public async Task AdminDeleteGenreAsync_MissingGenre_ThrowsKeyNotFound()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync((Genre?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => builder.Build().AdminDeleteGenreAsync(5));
        }

        [Fact]
        public async Task AdminDeleteGenreAsync_GenreUsedByGames_ThrowsAndKeepsGenre()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync(new Genre { Id = 5, Name = "Action" });
            builder.Games.Setup(g => g.GameExistsByGenreIdAsync(5)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => builder.Build().AdminDeleteGenreAsync(5));

            builder.Genres.Verify(g => g.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AdminDeleteGenreAsync_UnusedGenre_Deletes()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync(new Genre { Id = 5, Name = "Sports" });
            builder.Games.Setup(g => g.GameExistsByGenreIdAsync(5)).ReturnsAsync(false);

            await builder.Build().AdminDeleteGenreAsync(5);

            builder.Genres.Verify(g => g.DeleteAsync(5), Times.Once);
        }

        [Fact]
        public async Task GetGenreByIdAsync_MissingGenre_ThrowsKeyNotFound()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync((Genre?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => builder.Build().GetGenreByIdAsync(5));
        }

        [Fact]
        public async Task GetGenreByIdAsync_ReturnsLookup()
        {
            var builder = new AdminServiceBuilder();
            builder.Genres.Setup(g => g.GetByIdAsync(5)).ReturnsAsync(new Genre { Id = 5, Name = "Sports" });

            var lookup = await builder.Build().GetGenreByIdAsync(5);

            Assert.Equal(5, lookup.Id);
            Assert.Equal("Sports", lookup.Name);
        }
    }
}
