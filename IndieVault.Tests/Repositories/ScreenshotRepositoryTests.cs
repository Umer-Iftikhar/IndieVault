using IndieVault.Models;
using IndieVault.Repositories.Implementations;
using IndieVault.Tests.TestHelpers;

namespace IndieVault.Tests.Repositories
{
    public class ScreenshotRepositoryTests
    {
        [Fact]
        public async Task AddScreenshotsAsync_SavesAll_AndGetByGameIdReturnsThem()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var game = data.AddGame();
            var repository = new ScreenshotRepository(context);

            await repository.AddScreenshotsAsync(new List<Screenshot>
            {
                new() { GameId = game.Id, ImagePath = "/images/games/1/screen_1.png" },
                new() { GameId = game.Id, ImagePath = "/images/games/1/screen_2.png" }
            });

            var screenshots = await repository.GetScreenshotsByGameIdAsync(game.Id);
            Assert.Equal(2, screenshots.Count);
        }

        [Fact]
        public async Task DeleteScreenshotsByGameIdAsync_RemovesOnlyThatGamesScreenshots()
        {
            using var context = InMemoryContext.Create();
            var data = new TestData(context);
            var first = data.AddGame("First");
            var second = data.AddGame("Second");
            context.Screenshots.AddRange(
                new Screenshot { GameId = first.Id, ImagePath = "a.png" },
                new Screenshot { GameId = first.Id, ImagePath = "b.png" },
                new Screenshot { GameId = second.Id, ImagePath = "c.png" });
            context.SaveChanges();
            var repository = new ScreenshotRepository(context);

            await repository.DeleteScreenshotsByGameIdAsync(first.Id);

            Assert.Empty(await repository.GetScreenshotsByGameIdAsync(first.Id));
            Assert.Single(await repository.GetScreenshotsByGameIdAsync(second.Id));
        }
    }
}
