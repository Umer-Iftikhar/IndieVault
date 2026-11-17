using IndieVault.DTOs;
using IndieVault.Enums;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using Moq;

namespace IndieVault.Tests.Services
{
    public class GameBrowseServiceTests
    {
        private readonly Mock<IGameBrowseRepository> _repository = new();
        private readonly GameBrowseService _service;

        public GameBrowseServiceTests() => _service = new GameBrowseService(_repository.Object);

        [Fact]
        public async Task GetBrowseGamesAsync_PassesEveryFilterToRepository()
        {
            var platforms = new List<int> { 1, 2 };
            var expected = (new List<GameBrowseDto> { new() { Id = 1 } }, 1);
            _repository.Setup(r => r.GetPagedGamesAsync(2, 12, "knight", 5m, 30m, 4, platforms, SortBy.PriceAsc))
                .ReturnsAsync(expected);

            var (games, total) = await _service.GetBrowseGamesAsync(2, 12, "knight", 5m, 30m, 4, platforms, SortBy.PriceAsc);

            Assert.Single(games);
            Assert.Equal(1, total);
        }

        [Fact]
        public async Task GetFeaturedGamesAsync_ReturnsRepositoryResult()
        {
            var featured = new List<FeaturedGameDto> { new() { Id = 1, Title = "Featured" } };
            _repository.Setup(r => r.GetFeaturedGamesAsync()).ReturnsAsync(featured);

            Assert.Same(featured, await _service.GetFeaturedGamesAsync());
        }

        [Fact]
        public async Task GenreAndPlatformLists_ComeFromLookups()
        {
            _repository.Setup(r => r.GetGenreLookupsAsync()).ReturnsAsync(new List<LookupDto> { new() { Id = 1, Name = "Action" } });
            _repository.Setup(r => r.GetPlatformLookupsAsync()).ReturnsAsync(new List<LookupDto> { new() { Id = 2, Name = "Linux" } });

            Assert.Equal("Action", Assert.Single(await _service.GetGenreListAsync()).Name);
            Assert.Equal("Linux", Assert.Single(await _service.GetPlatformListAsync()).Name);
        }
    }
}
