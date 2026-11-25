using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class DownloadControllerTests
    {
        private readonly Mock<IDownloadService> _service = new();

        private DownloadController Controller(string? userId) => new(_service.Object)
        {
            ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User(userId, userId, "Player"))
        };

        [Fact]
        public async Task Download_RecordsForCurrentUserAndRedirectsToLink()
        {
            _service.Setup(s => s.DownloadGameAsync(3, "player-1")).ReturnsAsync("https://example.com/game.zip");

            var result = await Controller("player-1").Download(3, CancellationToken.None);

            Assert.Equal("https://example.com/game.zip", Assert.IsType<RedirectResult>(result).Url);
        }

        [Fact]
        public async Task MyDownloads_ReturnsCurrentUsersHistory()
        {
            var history = new List<DownloadHistoryDto> { new() { GameId = 3, GameName = "Celeste" } };
            _service.Setup(s => s.GetDownloadHistoryAsync("player-1")).ReturnsAsync(history);

            var result = await Controller("player-1").MyDownloads(CancellationToken.None);

            Assert.Same(history, Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task MyDownloads_NoUserId_ReturnsUnauthorized()
        {
            var result = await Controller(null).MyDownloads(CancellationToken.None);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Controller_AllowsAllSignedInRoles()
        {
            var auth = typeof(DownloadController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single();

            Assert.Equal("Player,Admin,GameDev", auth.Roles);
        }
    }
}
