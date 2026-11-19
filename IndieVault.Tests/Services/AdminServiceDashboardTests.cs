using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using Moq;

namespace IndieVault.Tests.Services
{
    public class AdminServiceDashboardTests
    {
        [Fact]
        public async Task GetAdminDashboardDataAsync_CombinesStatisticsAndLookups()
        {
            var builder = new AdminServiceBuilder();
            builder.Statistics.Setup(s => s.MostWishlistedGameAsync())
                .ReturnsAsync(new MostWishlistedGameDto { Title = "Hades", WishlistCount = 12 });
            builder.Statistics.Setup(s => s.GetUsersByRoleAsync()).ReturnsAsync(new List<UserByRoleDto>
            {
                new() { RoleName = "Player", UserCount = 20 },
                new() { RoleName = "GameDev", UserCount = 10 }
            });
            builder.Genres.Setup(g => g.GetAllAsync()).ReturnsAsync(new List<Genre> { new() { Id = 1, Name = "Action" } });
            builder.Games.Setup(g => g.GetAllAsync()).ReturnsAsync(new List<Game>
            {
                new() { Id = 1, Title = "Hades", IsFeatured = true },
                new() { Id = 2, Title = "Celeste", IsFeatured = false }
            });
            builder.Games.Setup(g => g.CountGamesAsync()).ReturnsAsync(2);
            builder.Reviews.Setup(r => r.CountReviewsAsync()).ReturnsAsync(7);

            var dashboard = await builder.Build().GetAdminDashboardDataAsync();

            Assert.Equal(2, dashboard.TotalGames);
            Assert.Equal(7, dashboard.TotalReviews);
            Assert.Equal("Hades", dashboard.MostWishlistedGame!.Title);
            Assert.Equal(2, dashboard.UsersByRole.Count);
            Assert.Equal("Action", Assert.Single(dashboard.Genres).Name);
            Assert.Collection(dashboard.Games,
                g => { Assert.Equal("Hades", g.Title); Assert.True(g.IsFeatured); },
                g => { Assert.Equal("Celeste", g.Title); Assert.False(g.IsFeatured); });
        }

        [Fact]
        public async Task GetAdminDashboardDataAsync_NoWishlists_LeavesMostWishlistedNull()
        {
            var builder = new AdminServiceBuilder();
            builder.Statistics.Setup(s => s.MostWishlistedGameAsync()).ReturnsAsync((MostWishlistedGameDto?)null);
            builder.Statistics.Setup(s => s.GetUsersByRoleAsync()).ReturnsAsync(new List<UserByRoleDto>());
            builder.Genres.Setup(g => g.GetAllAsync()).ReturnsAsync(new List<Genre>());
            builder.Games.Setup(g => g.GetAllAsync()).ReturnsAsync(new List<Game>());

            var dashboard = await builder.Build().GetAdminDashboardDataAsync();

            Assert.Null(dashboard.MostWishlistedGame);
            Assert.Empty(dashboard.Games);
        }

        [Fact]
        public async Task IsGameFeatureAsync_MissingGame_ThrowsKeyNotFound()
        {
            var builder = new AdminServiceBuilder();
            builder.Games.Setup(g => g.GetByIdAsync(3)).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => builder.Build().IsGameFeatureAsync(3));
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task IsGameFeatureAsync_TogglesFeaturedAndSaves(bool before, bool after)
        {
            var game = new Game { Id = 3, IsFeatured = before };
            var builder = new AdminServiceBuilder();
            builder.Games.Setup(g => g.GetByIdAsync(3)).ReturnsAsync(game);

            await builder.Build().IsGameFeatureAsync(3);

            Assert.Equal(after, game.IsFeatured);
            builder.Games.Verify(g => g.UpdateAsync(game), Times.Once);
        }
    }
}
