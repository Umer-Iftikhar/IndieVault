using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviews = new();
        private readonly Mock<IGameRepository> _games = new();
        private readonly ReviewService _service;

        public ReviewServiceTests()
        {
            _service = new ReviewService(_reviews.Object, _games.Object, NullLogger<ReviewService>.Instance);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task HasUserReviewedAsync_ReturnsRepositoryAnswer(bool exists)
        {
            _reviews.Setup(r => r.ReviewExistsAsync("player-1", 3)).ReturnsAsync(exists);

            Assert.Equal(exists, await _service.HasUserReviewedAsync(3, "player-1"));
        }

        [Fact]
        public async Task GetReviewFormAsync_GameMissing_ThrowsKeyNotFound()
        {
            _games.Setup(g => g.ExistsAsync(3)).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetReviewFormAsync(3));
        }

        [Fact]
        public async Task GetReviewFormAsync_GameExists_DoesNotThrow()
        {
            _games.Setup(g => g.ExistsAsync(3)).ReturnsAsync(true);

            await _service.GetReviewFormAsync(3);
        }

        [Fact]
        public async Task CreateReviewAsync_SavesReviewForUser()
        {
            Review? saved = null;
            _reviews.Setup(r => r.CreateAsync(It.IsAny<Review>()))
                .Callback<Review>(r => saved = r)
                .ReturnsAsync((Review r) => r);

            await _service.CreateReviewAsync(new CreateReviewDto { GameId = 3, Rating = 4, Comment = "Nice" }, "player-1");

            Assert.NotNull(saved);
            Assert.Equal(3, saved!.GameId);
            Assert.Equal("player-1", saved.UserId);
            Assert.Equal(4, saved.Rating);
            Assert.Equal("Nice", saved.Comment);
        }

        [Fact]
        public async Task CreateReviewAsync_RepositoryFails_Rethrows()
        {
            _reviews.Setup(r => r.CreateAsync(It.IsAny<Review>())).ThrowsAsync(new InvalidOperationException("db down"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateReviewAsync(new CreateReviewDto { GameId = 3, Rating = 4 }, "player-1"));
        }

        [Fact]
        public async Task DeleteReviewAsync_ReviewMissing_ThrowsKeyNotFound()
        {
            _reviews.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Review?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteReviewAsync(10, "player-1", isAdmin: false));
        }

        [Fact]
        public async Task DeleteReviewAsync_OtherUserNotAdmin_ThrowsAndKeepsReview()
        {
            _reviews.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Review { Id = 10, UserId = "player-1" });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteReviewAsync(10, "player-2", isAdmin: false));

            _reviews.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("player-1", false)] // author
        [InlineData("admin-1", true)]   // admin
        public async Task DeleteReviewAsync_AuthorOrAdmin_DeletesReview(string userId, bool isAdmin)
        {
            _reviews.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Review { Id = 10, UserId = "player-1" });

            await _service.DeleteReviewAsync(10, userId, isAdmin);

            _reviews.Verify(r => r.DeleteAsync(10), Times.Once);
        }
    }
}
