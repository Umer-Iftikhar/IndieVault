using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class AdminControllerGenreTests
    {
        private readonly Mock<IAdminService> _service = new();
        private readonly AdminController _controller;

        public AdminControllerGenreTests()
        {
            _controller = new AdminController(_service.Object, NullLogger<AdminController>.Instance)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("admin-1", "admin", "Admin"))
            };
        }

        [Fact]
        public async Task CreateGenre_Get_ReturnsEmptyForm()
        {
            var result = await _controller.CreateGenre();

            Assert.IsType<GenreViewModel>(Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task CreateGenre_Post_Success_RedirectsToDashboard()
        {
            var result = await _controller.CreateGenre(new GenreViewModel { GenreName = "Metroidvania" });

            Assert.Equal(nameof(AdminController.Dashboard), Assert.IsType<RedirectToActionResult>(result).ActionName);
            _service.Verify(s => s.AdminCreateGenreAsync("Metroidvania"), Times.Once);
        }

        [Fact]
        public async Task CreateGenre_Post_Duplicate_ShowsErrorOnForm()
        {
            _service.Setup(s => s.AdminCreateGenreAsync("Action")).ThrowsAsync(new InvalidOperationException("Genre already exists."));

            var result = await _controller.CreateGenre(new GenreViewModel { GenreName = "Action" });

            Assert.IsType<ViewResult>(result);
            Assert.Contains(_controller.ModelState[string.Empty]!.Errors, e => e.ErrorMessage.Contains("Genre already exists."));
        }

        [Fact]
        public async Task CreateGenre_Post_InvalidModel_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("GenreName", "Required");

            var result = await _controller.CreateGenre(new GenreViewModel());

            Assert.IsType<ViewResult>(result);
            _service.Verify(s => s.AdminCreateGenreAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GenreDelete_Get_ShowsGenreToConfirm()
        {
            _service.Setup(s => s.GetGenreByIdAsync(5)).ReturnsAsync(new LookupDto { Id = 5, Name = "Sports" });

            var result = await _controller.GenreDelete(5);

            var model = Assert.IsType<GenreViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(5, model.GenreId);
            Assert.Equal("Sports", model.GenreName);
        }

        [Fact]
        public async Task GenreDelete_Post_GenreInUse_ShowsMessage()
        {
            _service.Setup(s => s.AdminDeleteGenreAsync(5))
                .ThrowsAsync(new InvalidOperationException("Cannot delete genre because it is associated with existing games."));

            var result = await _controller.GenreDelete(new GenreViewModel { GenreId = 5, GenreName = "Action" });

            var model = Assert.IsType<GenreViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("Cannot delete genre because it is associated with existing games.", model.Message);
        }

        [Fact]
        public async Task GenreDelete_Post_Success_RedirectsToDashboard()
        {
            var result = await _controller.GenreDelete(new GenreViewModel { GenreId = 5 });

            Assert.Equal(nameof(AdminController.Dashboard), Assert.IsType<RedirectToActionResult>(result).ActionName);
            _service.Verify(s => s.AdminDeleteGenreAsync(5), Times.Once);
        }
    }
}
