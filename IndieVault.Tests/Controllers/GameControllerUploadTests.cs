using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class GameControllerUploadTests
    {
        private readonly Mock<IGameService> _gameService = new();
        private readonly GameController _controller;

        public GameControllerUploadTests()
        {
            _gameService.Setup(s => s.GetFormDataAsync()).ReturnsAsync(new GameFormDataDto
            {
                Genres = new List<LookupDto> { new() { Id = 1, Name = "Action" } },
                Engines = new List<LookupDto> { new() { Id = 1, Name = "Unity" } },
                Platforms = new List<LookupDto> { new() { Id = 1, Name = "Windows" } },
                Tags = new List<LookupDto> { new() { Id = 1, Name = "2D" } }
            });
            _controller = new GameController(_gameService.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("dev-1", "devone", "GameDev"))
            };
        }

        private static GameUploadViewModel Model(int screenshots = 0) => new()
        {
            Title = "Celeste",
            Description = "Climb the mountain.",
            Price = 19.99m,
            SelectedGenreId = 1,
            SelectedEngineId = 1,
            DownloadLink = "https://example.com/celeste",
            ReleaseDate = new DateTime(2018, 1, 25),
            CoverImage = FakeFormFile.Create("cover.png"),
            Screenshots = Enumerable.Range(1, screenshots).Select(i => FakeFormFile.Create($"s{i}.png")).ToList(),
            SelectedPlatforms = new List<string> { "1" },
            SelectedTags = new List<string> { "1" }
        };

        [Fact]
        public async Task Upload_Get_ReturnsFormWithLookupsLoaded()
        {
            var result = await _controller.Upload();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GameUploadViewModel>(view.Model);
            Assert.Single(model.Genres);
            Assert.Single(model.Engines);
            Assert.Single(model.Platforms);
            Assert.Single(model.Tags);
        }

        [Fact]
        public async Task Upload_Post_InvalidModel_ReturnsFormWithLookupsAndDoesNotUpload()
        {
            _controller.ModelState.AddModelError("Title", "Required");

            var result = await _controller.Upload(Model());

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotEmpty(Assert.IsType<GameUploadViewModel>(view.Model).Genres);
            _gameService.Verify(s => s.UploadGameAsync(It.IsAny<GameUploadDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Upload_Post_MoreThanFiveScreenshots_AddsErrorAndDoesNotUpload()
        {
            var result = await _controller.Upload(Model(screenshots: 6));

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Maximum 5 screenshots allowed.", _controller.ModelState["Screenshots"]!.Errors.Single().ErrorMessage);
            _gameService.Verify(s => s.UploadGameAsync(It.IsAny<GameUploadDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Upload_Post_Valid_UploadsForCurrentDeveloperAndRedirectsToMyGames()
        {
            var model = Model(screenshots: 5);

            var result = await _controller.Upload(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(GameController.MyGames), redirect.ActionName);
            _gameService.Verify(s => s.UploadGameAsync(It.Is<GameUploadDto>(d =>
                d.Title == "Celeste" &&
                d.Price == 19.99m &&
                d.CoverImage == model.CoverImage &&
                d.Screenshots!.Count == 5 &&
                d.SelectedPlatforms.SequenceEqual(new[] { "1" })), "dev-1"), Times.Once);
        }

        [Fact]
        public async Task MyGames_ReturnsCurrentDevelopersGames()
        {
            var games = new List<MyGameDto> { new() { Id = 1, Title = "Celeste" } };
            _gameService.Setup(s => s.GetMyGamesAsync("dev-1")).ReturnsAsync(games);

            var result = await _controller.MyGames();

            Assert.Same(games, Assert.IsType<ViewResult>(result).Model);
        }
    }
}
