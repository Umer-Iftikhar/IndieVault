using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class ReviewControllerTests
    {
        private readonly Mock<IReviewService> _service = new();

        private ReviewController Controller(string userId = "player-1", params string[] roles)
        {
            return new ReviewController(_service.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User(userId, userId, roles.Length == 0 ? new[] { "Player" } : roles))
            }.WithTempData();
        }

        private static void AssertRedirectsToGame(IActionResult result, int gameId)
        {
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Game", redirect.ControllerName);
            Assert.Equal(gameId, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Get_AlreadyReviewed_RedirectsWithError()
        {
            _service.Setup(s => s.HasUserReviewedAsync(3, "player-1")).ReturnsAsync(true);
            var controller = Controller();

            var result = await controller.Create(3);

            AssertRedirectsToGame(result, 3);
            Assert.Equal("You have already reviewed this game.", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Create_Get_NotReviewed_ShowsFormForGame()
        {
            _service.Setup(s => s.HasUserReviewedAsync(3, "player-1")).ReturnsAsync(false);

            var result = await Controller().Create(3);

            var model = Assert.IsType<ReviewViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(3, model.GameId);
            _service.Verify(s => s.GetReviewFormAsync(3), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsForm()
        {
            var controller = Controller();
            controller.ModelState.AddModelError("Rating", "Rating must be between 1 and 5.");

            var result = await controller.Create(new ReviewViewModel { GameId = 3, Rating = 9 });

            Assert.IsType<ViewResult>(result);
            _service.Verify(s => s.CreateReviewAsync(It.IsAny<CreateReviewDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_AlreadyReviewed_DoesNotCreateSecondReview()
        {
            _service.Setup(s => s.HasUserReviewedAsync(3, "player-1")).ReturnsAsync(true);

            var result = await Controller().Create(new ReviewViewModel { GameId = 3, Rating = 5 });

            AssertRedirectsToGame(result, 3);
            _service.Verify(s => s.CreateReviewAsync(It.IsAny<CreateReviewDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_Valid_CreatesReviewAndRedirectsToGame()
        {
            _service.Setup(s => s.HasUserReviewedAsync(3, "player-1")).ReturnsAsync(false);

            var result = await Controller().Create(new ReviewViewModel { GameId = 3, Rating = 5, Comment = "Loved it" });

            AssertRedirectsToGame(result, 3);
            _service.Verify(s => s.CreateReviewAsync(
                It.Is<CreateReviewDto>(d => d.GameId == 3 && d.Rating == 5 && d.Comment == "Loved it"), "player-1"), Times.Once);
        }

        [Theory]
        [InlineData("Player", false)]
        [InlineData("Admin", true)]
        public async Task Delete_PassesAdminFlagFromRoleAndRedirectsToGame(string role, bool expectedAdmin)
        {
            var controller = Controller("user-1", role);

            var result = await controller.Delete(10, gameId: 3);

            AssertRedirectsToGame(result, 3);
            _service.Verify(s => s.DeleteReviewAsync(10, "user-1", expectedAdmin), Times.Once);
            Assert.Equal("Review removed successfully.", controller.TempData["Message"]);
        }
    }
}
