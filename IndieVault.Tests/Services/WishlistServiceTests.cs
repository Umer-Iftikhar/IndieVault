using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.Services
{
    public class WishlistServiceTests
    {
        private readonly Mock<IWishlistRepository> _repository = new();
        private readonly WishlistService _service;

        public WishlistServiceTests()
        {
            _service = new WishlistService(_repository.Object, NullLogger<WishlistService>.Instance);
        }

        [Fact]
        public async Task AddToWishlistAsync_AlreadyWishlisted_ThrowsAndDoesNotAdd()
        {
            _repository.Setup(r => r.WishlistExistsAsync("player-1", 3)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddToWishlistAsync(3, "player-1"));

            Assert.Equal("Game is already in the wishlist.", ex.Message);
            _repository.Verify(r => r.CreateAsync(It.IsAny<Wishlist>()), Times.Never);
        }

        [Fact]
        public async Task AddToWishlistAsync_NewGame_CreatesEntryForUser()
        {
            _repository.Setup(r => r.WishlistExistsAsync("player-1", 3)).ReturnsAsync(false);

            await _service.AddToWishlistAsync(3, "player-1");

            _repository.Verify(r => r.CreateAsync(It.Is<Wishlist>(w => w.GameId == 3 && w.UserId == "player-1")), Times.Once);
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_NotInWishlist_Throws()
        {
            _repository.Setup(r => r.GetWishlistEntryAsync("player-1", 3)).ReturnsAsync((Wishlist?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoveFromWishlistAsync(3, "player-1"));
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_InWishlist_DeletesThatEntry()
        {
            _repository.Setup(r => r.GetWishlistEntryAsync("player-1", 3)).ReturnsAsync(new Wishlist { Id = 42, GameId = 3, UserId = "player-1" });

            await _service.RemoveFromWishlistAsync(3, "player-1");

            _repository.Verify(r => r.DeleteAsync(42), Times.Once);
        }

        [Fact]
        public async Task GetUserWishlistAsync_MapsGameDetails()
        {
            var added = new DateTime(2026, 5, 1);
            _repository.Setup(r => r.GetWishlistByUserIdAsync("player-1")).ReturnsAsync(new List<Wishlist>
            {
                new()
                {
                    GameId = 3, CreatedDate = added,
                    Game = new Game { Title = "Celeste", Price = 19.99m, CoverImagePath = "/c.png", Genre = new Genre { Name = "Platformer" } }
                }
            });

            var items = await _service.GetUserWishlistAsync("player-1");

            var item = Assert.Single(items);
            Assert.Equal(3, item.GameId);
            Assert.Equal("Celeste", item.Title);
            Assert.Equal("Platformer", item.GameGenre);
            Assert.Equal(19.99m, item.Price);
            Assert.Equal("/c.png", item.CoverImagePath);
            Assert.Equal(added, item.CreatedDate);
        }
    }
}
