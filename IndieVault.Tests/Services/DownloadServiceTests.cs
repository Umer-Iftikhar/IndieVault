using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Services
{
    public class DownloadServiceTests
    {
        private readonly Mock<IGameRepository> _games = new();
        private readonly Mock<IDownloadRepository> _downloads = new();
        private readonly DownloadService _service;

        public DownloadServiceTests()
        {
            _service = new DownloadService(_games.Object, _downloads.Object, NullLogger<DownloadService>.Instance);
        }

        [Fact]
        public async Task DownloadGameAsync_MissingGame_ThrowsAndRecordsNothing()
        {
            _games.Setup(g => g.GetByIdAsync(3)).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DownloadGameAsync(3, "player-1"));

            _downloads.Verify(d => d.CreateAsync(It.IsAny<DownloadHistory>()), Times.Never);
        }

        [Fact]
        public async Task DownloadGameAsync_RecordsDownloadAndReturnsLink()
        {
            _games.Setup(g => g.GetByIdAsync(3)).ReturnsAsync(new Game { Id = 3, DownloadLink = "https://example.com/game.zip" });
            var before = DateTime.UtcNow.AddSeconds(-1);

            var link = await _service.DownloadGameAsync(3, "player-1");

            Assert.Equal("https://example.com/game.zip", link);
            _downloads.Verify(d => d.CreateAsync(It.Is<DownloadHistory>(h =>
                h.GameId == 3 && h.UserId == "player-1" && h.DownloadDate >= before)), Times.Once);
        }

        [Fact]
        public async Task GetDownloadHistoryAsync_MapsGameNameAndTime()
        {
            var when = new DateTime(2026, 5, 20, 10, 30, 0);
            _downloads.Setup(d => d.GetDownloadHistoryByUserIdAsync("player-1")).ReturnsAsync(new List<DownloadHistory>
            {
                new() { GameId = 3, DownloadDate = when, Game = new Game { Id = 3, Title = "Celeste" } }
            });

            var history = await _service.GetDownloadHistoryAsync("player-1");

            var entry = Assert.Single(history);
            Assert.Equal(3, entry.GameId);
            Assert.Equal("Celeste", entry.GameName);
            Assert.Equal(when, entry.DownloadTime);
        }
    }
}
