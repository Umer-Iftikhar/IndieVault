using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Moq;

namespace IndieVault.Tests.Services
{
    public class GameServiceUploadTests
    {
        private static GameUploadDto UploadDto(UploadFiles files) => new()
        {
            Title = "Celeste",
            Description = "Climb the mountain.",
            Price = 19.99m,
            ReleaseDate = new DateTime(2018, 1, 25),
            DownloadLink = "https://example.com/celeste",
            SelectedGenreId = 2,
            SelectedEngineId = 3,
            SelectedPlatforms = new List<string> { "1", "4" },
            SelectedTags = new List<string> { "5" },
            CoverImage = files.Cover!,
            Screenshots = files.Screenshots
        };

        private class UploadFiles
        {
            public Microsoft.AspNetCore.Http.IFormFile? Cover { get; init; }
            public List<Microsoft.AspNetCore.Http.IFormFile>? Screenshots { get; init; }
        }

        [Fact]
        public async Task GetFormDataAsync_MapsAllLookupsToDtos()
        {
            var builder = new GameServiceBuilder();
            builder.Genres.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre> { new() { Id = 1, Name = "Action" } });
            builder.Engines.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Engine> { new() { Id = 2, Name = "Godot" } });
            builder.Platforms.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Platform> { new() { Id = 3, Name = "Linux" }, new() { Id = 4, Name = "Windows" } });
            builder.Tags.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Tag>());

            var formData = await builder.Build().GetFormDataAsync();

            Assert.Equal("Action", Assert.Single(formData.Genres).Name);
            Assert.Equal(2, Assert.Single(formData.Engines).Id);
            Assert.Equal(2, formData.Platforms.Count);
            Assert.Empty(formData.Tags);
        }

        [Fact]
        public async Task UploadGameAsync_CreatesGameWithSelectedPlatformsAndTags()
        {
            using var webRoot = new TempWebRoot();
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path);
            Game? created = null;
            builder.Games.Setup(g => g.CreateAsync(It.IsAny<Game>()))
                .Callback<Game>(g => created = g)
                .ReturnsAsync((Game g) => { g.Id = 7; return g; });

            await builder.Build().UploadGameAsync(UploadDto(new UploadFiles()), "dev-1");

            Assert.NotNull(created);
            Assert.Equal("Celeste", created!.Title);
            Assert.Equal("dev-1", created.DeveloperId);
            Assert.Equal(2, created.GenreId);
            Assert.Equal(3, created.EngineId);
            Assert.Equal(new[] { 1, 4 }, created.GamePlatforms.Select(p => p.PlatformId));
            Assert.Equal(new[] { 5 }, created.GameTags.Select(t => t.TagId));
            builder.Games.Verify(g => g.UpdateAsync(created!), Times.Once);
        }

        [Fact]
        public async Task UploadGameAsync_SavesCoverImageInGameFolder()
        {
            using var webRoot = new TempWebRoot();
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path).AssignIdOnCreate(7);
            Game? updated = null;
            builder.Games.Setup(g => g.UpdateAsync(It.IsAny<Game>())).Callback<Game>(g => updated = g).Returns(Task.CompletedTask);

            await builder.Build().UploadGameAsync(
                UploadDto(new UploadFiles { Cover = FakeFormFile.Create("box-art.png") }), "dev-1");

            Assert.True(File.Exists(Path.Combine(webRoot.GameFolder(7), "cover.png")));
            Assert.Equal("/images/games/7/cover.png", updated!.CoverImagePath);
        }

        [Fact]
        public async Task UploadGameAsync_SavesScreenshotsAndRecordsThem()
        {
            using var webRoot = new TempWebRoot();
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path).AssignIdOnCreate(9);
            List<Screenshot>? saved = null;
            builder.Screenshots.Setup(s => s.AddScreenshotsAsync(It.IsAny<List<Screenshot>>()))
                .Callback<List<Screenshot>>(list => saved = list)
                .Returns(Task.CompletedTask);
            var files = new UploadFiles
            {
                Cover = FakeFormFile.Create("cover.jpg"),
                Screenshots = new List<Microsoft.AspNetCore.Http.IFormFile>
                {
                    FakeFormFile.Create("a.jpg"),
                    FakeFormFile.Create("b.png")
                }
            };

            await builder.Build().UploadGameAsync(UploadDto(files), "dev-1");

            Assert.NotNull(saved);
            Assert.Equal(new[] { "/images/games/9/screen_1.jpg", "/images/games/9/screen_2.png" }, saved!.Select(s => s.ImagePath));
            Assert.All(saved, s => Assert.Equal(9, s.GameId));
            Assert.True(File.Exists(Path.Combine(webRoot.GameFolder(9), "screen_1.jpg")));
            Assert.True(File.Exists(Path.Combine(webRoot.GameFolder(9), "screen_2.png")));
        }

        [Fact]
        public async Task UploadGameAsync_WithoutScreenshots_DoesNotSaveScreenshotRecords()
        {
            using var webRoot = new TempWebRoot();
            var builder = new GameServiceBuilder().WithWebRoot(webRoot.Path).AssignIdOnCreate(3);

            await builder.Build().UploadGameAsync(UploadDto(new UploadFiles { Cover = FakeFormFile.Create("c.png") }), "dev-1");

            builder.Screenshots.Verify(s => s.AddScreenshotsAsync(It.IsAny<List<Screenshot>>()), Times.Never);
        }

        [Fact]
        public async Task GetMyGamesAsync_MapsGenreAndEngineNames()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGamesByDevIdAsync("dev-1")).ReturnsAsync(new List<Game>
            {
                new()
                {
                    Id = 1, Title = "Hades", Price = 24.99m, CoverImagePath = "/c.png",
                    Genre = new Genre { Name = "Action" }, Engine = new Engine { Name = "Unknown" }
                }
            });

            var games = await builder.Build().GetMyGamesAsync("dev-1");

            var game = Assert.Single(games);
            Assert.Equal("Hades", game.Title);
            Assert.Equal("Action", game.GenreName);
            Assert.Equal("Unknown", game.Engine);
        }
    }
}
