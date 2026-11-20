using IndieVault.Tests.TestHelpers;
using Moq;

namespace IndieVault.Tests.Services
{
    public class GameServiceDeveloperCountTests
    {
        [Fact]
        public async Task GetDevGameCountAsync_ReturnsRepositoryCount()
        {
            var builder = new GameServiceBuilder();
            builder.Games.Setup(g => g.GetGameCountByDevIdAsync("dev-1")).ReturnsAsync(4);

            Assert.Equal(4, await builder.Build().GetDevGameCountAsync("dev-1"));
        }
    }
}
