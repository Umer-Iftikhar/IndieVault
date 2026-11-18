using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class WishlistControllerTests
    {
        private readonly Mock<IWishlistService> _service = new();
        private readonly WishlistController _controller;

        public WishlistControllerTests()
        {
            _controller = new WishlistController(_service.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("player-1", "playerone", "Player"))
            };
        }

        private static (bool Success, string Message) ReadJson(IActionResult result)
        {
            var value = Assert.IsType<JsonResult>(result).Value!;
            var type = value.GetType();
            return ((bool)type.GetProperty("success")!.GetValue(value)!, (string)type.GetProperty("message")!.GetValue(value)!);
        }

        [Fact]
        public async Task Add_AddsForCurrentPlayerAndReturnsSuccessJson()
        {
            var result = await _controller.Add(new WishlistRequestDto { GameId = 3 });

            var (success, message) = ReadJson(result);
            Assert.True(success);
            Assert.Equal("Game added to your wishlist!", message);
            _service.Verify(s => s.AddToWishlistAsync(3, "player-1"), Times.Once);
        }

        [Fact]
        public async Task Remove_RemovesForCurrentPlayerAndReturnsSuccessJson()
        {
            var result = await _controller.Remove(new WishlistRequestDto { GameId = 3 });

            var (success, message) = ReadJson(result);
            Assert.True(success);
            Assert.Equal("Game removed from your wishlist.", message);
            _service.Verify(s => s.RemoveFromWishlistAsync(3, "player-1"), Times.Once);
        }

        [Fact]
        public async Task ViewWishlist_ShowsCurrentPlayersItems()
        {
            var items = new List<WishlistItemDto> { new() { GameId = 3, Title = "Celeste" } };
            _service.Setup(s => s.GetUserWishlistAsync("player-1")).ReturnsAsync(items);

            var result = await _controller.ViewWishlist();

            Assert.Same(items, Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public void Controller_IsRestrictedToPlayers()
        {
            var auth = typeof(WishlistController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single();

            Assert.Equal("Player", auth.Roles);
        }
    }
}
