using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Enums;
using IndieVault.Services.Interfaces;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<IGameBrowseService> _browse = new();
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _browse.Setup(b => b.GetGenreListAsync()).ReturnsAsync(new List<LookupDto> { new() { Id = 1, Name = "Action" } });
            _browse.Setup(b => b.GetPlatformListAsync()).ReturnsAsync(new List<LookupDto> { new() { Id = 1, Name = "Windows" } });
            _browse.Setup(b => b.GetFeaturedGamesAsync()).ReturnsAsync(new List<FeaturedGameDto> { new() { Id = 9 } });
            _controller = new HomeController(_browse.Object);
        }

        private void SetupGames(int totalCount)
        {
            _browse.Setup(b => b.GetBrowseGamesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(), It.IsAny<int?>(), It.IsAny<List<int>?>(), It.IsAny<SortBy>()))
                .ReturnsAsync((new List<GameBrowseDto>(), totalCount));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(12, 1)]
        [InlineData(13, 2)]
        [InlineData(50, 5)]
        public async Task Index_CalculatesTotalPagesFromTotalCount(int totalCount, int expectedPages)
        {
            SetupGames(totalCount);

            var result = await _controller.Index(null, null, null, null, null);

            var model = Assert.IsType<GameBrowseViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(expectedPages, model.TotalPages);
            Assert.Equal(totalCount, model.TotalCount);
        }

        [Fact]
        public async Task Index_Defaults_FirstPageOf12SortedByNewest()
        {
            SetupGames(0);

            await _controller.Index(null, null, null, null, null);

            _browse.Verify(b => b.GetBrowseGamesAsync(1, 12, null, null, null, null, null, SortBy.Newest), Times.Once);
        }

        [Fact]
        public async Task Index_KeepsSelectedFiltersOnTheViewModel()
        {
            SetupGames(30);
            var platforms = new List<int> { 1, 3 };

            var result = await _controller.Index("knight", 5m, 25m, 2, platforms, SortBy.Rating, pageNumber: 3);

            var model = Assert.IsType<GameBrowseViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("knight", model.SearchTerm);
            Assert.Equal(5m, model.MinPrice);
            Assert.Equal(25m, model.MaxPrice);
            Assert.Equal(2, model.SelectedGenreId);
            Assert.Equal(platforms, model.SelectedPlatformsId);
            Assert.Equal(SortBy.Rating, model.SortBy);
            Assert.Equal(3, model.CurrentPage);
            Assert.Single(model.Genres);
            Assert.Single(model.Platforms);
            Assert.Single(model.FeaturedGames);
        }

        [Fact]
        public async Task Index_NoPlatformsSelected_UsesEmptyList()
        {
            SetupGames(0);

            var result = await _controller.Index(null, null, null, null, platformIds: null);

            var model = Assert.IsType<GameBrowseViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotNull(model.SelectedPlatformsId);
            Assert.Empty(model.SelectedPlatformsId);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            Assert.IsType<ViewResult>(_controller.Privacy());
        }
    }
}
