using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class AdminControllerDashboardTests
    {
        private readonly Mock<IAdminService> _service = new();
        private readonly AdminController _controller;

        public AdminControllerDashboardTests()
        {
            _controller = new AdminController(_service.Object, Mock.Of<IRawgApiService>(), NullLogger<AdminController>.Instance)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("admin-1", "admin", "Admin"))
            };
        }

        [Fact]
        public async Task Dashboard_MapsServiceDataToViewModel()
        {
            _service.Setup(s => s.GetAdminDashboardDataAsync()).ReturnsAsync(new AdminDashboardDto
            {
                TotalGames = 50,
                TotalReviews = 120,
                MostWishlistedGame = new MostWishlistedGameDto { Title = "Hades", WishlistCount = 9 },
                UsersByRole = new List<UserByRoleDto> { new() { RoleName = "Player", UserCount = 20 } },
                Genres = new List<LookupDto> { new() { Id = 1, Name = "Action" } },
                Games = new List<AdminGameDto> { new() { Id = 1, Title = "Hades", IsFeatured = true } }
            });

            var result = await _controller.Dashboard();

            var model = Assert.IsType<AdminDashboardViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("admin", model.AdminName);
            Assert.Equal(50, model.TotalGames);
            Assert.Equal(120, model.TotalReviews);
            Assert.Equal("Hades", model.MostWishlistedGames.Title);
            Assert.Single(model.UsersByRole);
            Assert.Single(model.Genres);
            Assert.True(Assert.Single(model.Games).IsFeatured);
        }

        [Fact]
        public async Task ToggleFeature_TogglesAndReturnsToDashboard()
        {
            var result = await _controller.ToggleFeature(4);

            Assert.Equal(nameof(AdminController.Dashboard), Assert.IsType<RedirectToActionResult>(result).ActionName);
            _service.Verify(s => s.IsGameFeatureAsync(4), Times.Once);
        }

        [Fact]
        public void Controller_IsRestrictedToAdmins()
        {
            var auth = typeof(AdminController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single();

            Assert.Equal("Admin", auth.Roles);
        }
    }
}
