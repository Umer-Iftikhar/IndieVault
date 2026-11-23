using IndieVault.Controllers;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class AdminControllerSyncTests
    {
        private readonly Mock<IRawgApiService> _rawg = new();
        private readonly AdminController _controller;

        public AdminControllerSyncTests()
        {
            _controller = new AdminController(Mock.Of<IAdminService>(), _rawg.Object, NullLogger<AdminController>.Instance)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("admin-1", "admin", "Admin"))
            }.WithTempData();
        }

        [Fact]
        public async Task SyncGames_Success_ReportsSyncedCount()
        {
            _rawg.Setup(r => r.SynchronizeGamesFromApiAsync()).ReturnsAsync(12);

            var result = await _controller.SyncGames();

            Assert.Equal(nameof(AdminController.Dashboard), Assert.IsType<RedirectToActionResult>(result).ActionName);
            Assert.Equal("12 games synced successfully.", _controller.TempData["Message"]);
        }

        [Fact]
        public async Task SyncGames_Failure_ReportsErrorAndStillRedirects()
        {
            _rawg.Setup(r => r.SynchronizeGamesFromApiAsync()).ThrowsAsync(new HttpRequestException("timeout"));

            var result = await _controller.SyncGames();

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RAWG sync failed. Check logs.", _controller.TempData["Error"]);
        }
    }
}
