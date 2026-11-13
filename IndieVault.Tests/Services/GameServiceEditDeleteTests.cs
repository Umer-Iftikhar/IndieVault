using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Moq;

namespace IndieVault.Tests.Services
{
    public class GameServiceEditDeleteTests
    {
        private static Game OwnedGame(int id = 5, string developerId = "dev-1") => new()
        {
            Id = id,
            Title = "Old title",
            Description = "Old description",
            Price = 10m,
            DeveloperId = developerId,
            GenreId = 1,
            EngineId = 1,
            CoverImagePath = $"/images/games/{id}/cover.png",
            DownloadLink = "https://example.com/old",
            GamePlatforms = new List<GamePlatform> { new() { PlatformId = 1 }, new() { PlatformId = 2 } },
            GameTags = new List<GameTag> { new() { TagId = 3 } }
        };

        private static GameUpdateDto Update(int id = 5, IFormFile? cover = null, List<IFormFile>? screenshots = null) => new()
        {
            Id = id,
            Title = "New title",
            Description = "New description",
            Price = 15m,
            ReleaseDate = new DateTime(2024, 6, 1),
            SelectedGenreId = 2,
            SelectedEngineId = 4,
            DownloadLink = "https://example.com/new",
            SelectedPlatforms = new List<string> { "3" },
            SelectedTags = new List<string> { "6", "7" },
            CoverImage = cover,
            Screenshots = screenshots
        };

        // ---- Delete ----

        [Fact]
        public async Task DeleteGameAsync_NotOwner_ThrowsAndDoesNotDelete()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameIfOwnerAsync(5, "intruder")).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => builder.Build().DeleteGameAsync(5, "intruder"));

            builder.Games.Verify(g => g.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteGameAsync_Owner_DeletesImageFolderAndGame()
        {
            using var webRoot = new TempWebRoot();
            Directory.CreateDirectory(webRoot.GameFolder(5));
            File.WriteAllText(Path.Combine(webRoot.GameFolder(5), "cover.png"), "x");
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path);
            builder.Games.Setup(g => g.GetGameIfOwnerAsync(5, "dev-1")).ReturnsAsync(OwnedGame());

            await builder.Build().DeleteGameAsync(5, "dev-1");

            Assert.False(Directory.Exists(webRoot.GameFolder(5)));
            builder.Games.Verify(g => g.DeleteAsync(5), Times.Once);
        }

        [Fact]
        public async Task DeleteGameAsync_Owner_NoImageFolder_StillDeletesGame()
        {
            using var webRoot = new TempWebRoot();
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path);
            builder.Games.Setup(g => g.GetGameIfOwnerAsync(5, "dev-1")).ReturnsAsync(OwnedGame());

            await builder.Build().DeleteGameAsync(5, "dev-1");

            builder.Games.Verify(g => g.DeleteAsync(5), Times.Once);
        }

        // ---- Get for edit ----

        [Fact]
        public async Task GetGameForEditAsync_MissingGame_Throws()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => builder.Build().GetGameForEditAsync(5, "dev-1"));
        }

        [Fact]
        public async Task GetGameForEditAsync_NotOwner_ThrowsUnauthorized()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync(OwnedGame(developerId: "dev-1"));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => builder.Build().GetGameForEditAsync(5, "dev-2"));
        }

        [Fact]
        public async Task GetGameForEditAsync_Owner_MapsCurrentValues()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync(OwnedGame());

            var dto = await builder.Build().GetGameForEditAsync(5, "dev-1");

            Assert.Equal("Old title", dto.Title);
            Assert.Equal("/images/games/5/cover.png", dto.CoverImagePath);
            Assert.Equal(new[] { 1, 2 }, dto.SelectedPlatformIds);
            Assert.Equal(new[] { 3 }, dto.SelectedTagIds);
        }

        // ---- Update ----

        [Fact]
        public async Task UpdateGameAsync_MissingGame_ThrowsArgumentException()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => builder.Build().UpdateGameAsync(Update(), "dev-1"));
        }

        [Fact]
        public async Task UpdateGameAsync_NotOwner_ThrowsAndDoesNotSave()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync(OwnedGame(developerId: "dev-1"));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => builder.Build().UpdateGameAsync(Update(), "dev-2"));

            builder.Games.Verify(g => g.UpdateAsync(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task UpdateGameAsync_Owner_UpdatesFieldsAndReplacesPlatformsAndTags()
        {
            var game = OwnedGame();
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync(game);

            await builder.Build().UpdateGameAsync(Update(), "dev-1");

            Assert.Equal("New title", game.Title);
            Assert.Equal(15m, game.Price);
            Assert.Equal(2, game.GenreId);
            Assert.Equal(4, game.EngineId);
            Assert.Equal("https://example.com/new", game.DownloadLink);
            Assert.Equal(new[] { 3 }, game.GamePlatforms.Select(p => p.PlatformId));
            Assert.Equal(new[] { 6, 7 }, game.GameTags.Select(t => t.TagId));
            Assert.Equal("/images/games/5/cover.png", game.CoverImagePath); // no new cover -> unchanged
            builder.Games.Verify(g => g.UpdateAsync(game), Times.Once);
            builder.Screenshots.Verify(s => s.DeleteScreenshotsByGameIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateGameAsync_NewCover_SavesFileAndUpdatesPath()
        {
            using var webRoot = new TempWebRoot();
            var game = OwnedGame();
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path);
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync(game);

            await builder.Build().UpdateGameAsync(Update(cover: FakeFormFile.Create("new.jpg")), "dev-1");

            Assert.Equal("/images/games/5/cover.jpg", game.CoverImagePath);
            Assert.True(File.Exists(Path.Combine(webRoot.GameFolder(5), "cover.jpg")));
        }

        [Fact]
        public async Task UpdateGameAsync_NewScreenshots_ReplacesOldScreenshotRecordsAndFiles()
        {
            using var webRoot = new TempWebRoot();
            var folder = webRoot.GameFolder(5);
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "screenshot_old.png"), "old");
            File.WriteAllText(Path.Combine(folder, "cover.png"), "cover");
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path);
            builder.Games.Setup(g => g.GetGameWithPlatformsAndTagsAsync(5)).ReturnsAsync(OwnedGame());
            List<Screenshot>? added = null;
            builder.Screenshots.Setup(s => s.AddScreenshotsAsync(It.IsAny<List<Screenshot>>()))
                .Callback<List<Screenshot>>(l => added = l)
                .Returns(Task.CompletedTask);

            await builder.Build().UpdateGameAsync(
                Update(screenshots: new List<IFormFile> { FakeFormFile.Create("one.png"), FakeFormFile.Create("two.png") }),
                "dev-1");

            builder.Screenshots.Verify(s => s.DeleteScreenshotsByGameIdAsync(5), Times.Once);
            Assert.False(File.Exists(Path.Combine(folder, "screenshot_old.png")));
            Assert.True(File.Exists(Path.Combine(folder, "cover.png"))); // cover is kept
            Assert.Equal(2, added!.Count);
            Assert.All(added, s => Assert.StartsWith("/images/games/5/screenshot_", s.ImagePath));
            Assert.Equal(2, Directory.GetFiles(folder, "screenshot_*").Length);
        }
    }
}
