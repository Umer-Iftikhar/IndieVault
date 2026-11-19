using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndieVault.Controllers
{
    [Authorize(Roles = "Player")]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] WishlistRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _wishlistService.AddToWishlistAsync(dto.GameId, userId!);

            return Json(new { success = true, message = "Game added to your wishlist!" });
        }

        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] WishlistRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if the game is in the user's wishlist before attempting to remove it
            await _wishlistService.RemoveFromWishlistAsync(dto.GameId, userId!);

            return Json (new { success = true, message = "Game removed from your wishlist." });
        }

        [HttpGet]
        public async Task<IActionResult> ViewWishlist ()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var wishlistGames = await _wishlistService.GetUserWishlistAsync(userId!);

            return View(wishlistGames);
        }
    }
}
