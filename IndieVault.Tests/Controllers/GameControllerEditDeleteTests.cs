using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class GameControllerEditDeleteTests
    {
        private readonly Mock<IGameService> _gameService = new();
        private readonly GameController _controller;

        public GameControllerEditDeleteTests()
        {
            _gameService.Setup(s => s.GetFormDataAsync()).ReturnsAsync(new GameFormDataDto
            {
                Genres = new List<LookupDto> { new() { Id = 1, Name = "Action" } }
            });
            _controller = new GameController(_gameService.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("dev-1", "devone", "GameDev"))
            };
        }

        private static GameEditViewModel EditModel(int screenshots = 0) => new()
        {
            Id = 5,
            Title = "New title",
            Description = "New description",
            Price = 15m,
            SelectedGenreId = 1,
            SelectedEngineId = 1,
            DownloadLink = "https://example.com/new",
            ReleaseDate = new DateTime(2024, 6, 1),
            Screenshots = Enumerable.Range(1, screenshots).Select(i => FakeFormFile.Create($"s{i}.png")).ToList(),
            SelectedPlatforms = new List<string> { "2" },
            SelectedTags = new List<string>()
        };

        [Fact]
        public async Task Delete_DeletesForCurrentUserAndRedirectsToMyGames()
        {
            var result = await _controller.Delete(5);

            Assert.Equal(nameof(GameController.MyGames), Assert.IsType<RedirectToActionResult>(result).ActionName);
            _gameService.Verify(s => s.DeleteGameAsync(5, "dev-1"), Times.Once);
        }

        [Fact]
        public async Task Edit_Get_MapsGameToViewModelWithStringIds()
        {
            _gameService.Setup(s => s.GetGameForEditAsync(5, "dev-1")).ReturnsAsync(new GameEditDto
            {
                Id = 5,
                Title = "Celeste",
                CoverImagePath = "/images/games/5/cover.png",
                SelectedGenreId = 1,
                SelectedPlatformIds = new List<int> { 1, 3 },
                SelectedTagIds = new List<int> { 2 }
            });

            var result = await _controller.Edit(5);

            var model = Assert.IsType<GameEditViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(5, model.Id);
            Assert.Equal("Celeste", model.Title);
            Assert.Equal("/images/games/5/cover.png", model.ExistingCoverImagePath);
            Assert.Null(model.CoverImage);
            Assert.Equal(new[] { "1", "3" }, model.SelectedPlatforms);
            Assert.Equal(new[] { "2" }, model.SelectedTags);
            Assert.Single(model.Genres);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsFormAndDoesNotUpdate()
        {
            _controller.ModelState.AddModelError("Price", "Price must be greater than 0.");

            var result = await _controller.Edit(EditModel());

            Assert.IsType<ViewResult>(result);
            _gameService.Verify(s => s.UpdateGameAsync(It.IsAny<GameUpdateDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_MoreThanFiveScreenshots_AddsError()
        {
            var result = await _controller.Edit(EditModel(screenshots: 6));

            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            _gameService.Verify(s => s.UpdateGameAsync(It.IsAny<GameUpdateDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_Valid_UpdatesAndRedirectsToMyGames()
        {
            var result = await _controller.Edit(EditModel());

            Assert.Equal(nameof(GameController.MyGames), Assert.IsType<RedirectToActionResult>(result).ActionName);
            _gameService.Verify(s => s.UpdateGameAsync(
                It.Is<GameUpdateDto>(d => d.Id == 5 && d.Title == "New title" && d.SelectedPlatforms.Single() == "2"),
                "dev-1"), Times.Once);
        }
    }
}
