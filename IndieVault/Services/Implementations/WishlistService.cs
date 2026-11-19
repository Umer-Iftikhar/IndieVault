using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Interfaces;

namespace IndieVault.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ILogger<WishlistService> _logger;
        public WishlistService(IWishlistRepository wishlistRepository, ILogger<WishlistService> logger)
        {
            _wishlistRepository = wishlistRepository;
            _logger = logger;
        }
        
        public async Task AddToWishlistAsync(int gameId, string userId)
        {
            _logger.LogInformation("User {UserId} adding game {GameId} to wishlist", userId, gameId);

            // Check if the user has already wishlisted the game
            if (await _wishlistRepository.WishlistExistsAsync(userId, gameId))
            {
                _logger.LogWarning("User {UserId} attempted to add game {GameId} which is already in wishlist", userId, gameId);
                throw new InvalidOperationException("Game is already in the wishlist.");
            }

            // Create a new wishlist entry
            var wishlistItem = new Wishlist
            {
                GameId = gameId,
                UserId = userId
            };

            // Save the wishlist entry to the database
            await _wishlistRepository.CreateAsync(wishlistItem);
            _logger.LogInformation("Game {GameId} added to wishlist for user {UserId}", gameId, userId);
        }
        public async Task RemoveFromWishlistAsync(int gameId, string userId)
        {
            _logger.LogInformation("User {UserId} removing game {GameId} from wishlist", userId, gameId);

            // Check if the wishlist entry exists
            var wishlistItem = await _wishlistRepository.GetWishlistEntryAsync(userId, gameId);

            if (wishlistItem == null)
            {
                _logger.LogWarning("Wishlist entry not found for user {UserId}, game {GameId}", userId, gameId);
                throw new InvalidOperationException("Wishlist entry not found.");
            }

            // Delete the wishlist entry from the database
            await _wishlistRepository.DeleteAsync(wishlistItem.Id);
            _logger.LogInformation("Game {GameId} removed from wishlist for user {UserId}", gameId, userId);
        }
        public async Task<List<WishlistItemDto>> GetUserWishlistAsync(string userId)
        {
            _logger.LogInformation("Retrieving wishlist for user {UserId}", userId);

            // Retrieve the wishlist items for the user
            var wishlistGames = await _wishlistRepository.GetWishlistByUserIdAsync(userId);

            // Map the wishlist items to WishlistItemDto and return the list
            return wishlistGames.Select(w => new WishlistItemDto
            {
                GameId = w.GameId,
                CoverImagePath = w.Game.CoverImagePath,
                CreatedDate = w.CreatedDate,
                GameGenre = w.Game.Genre.Name,
                Price = w.Game.Price,
                Title = w.Game.Title
            }).ToList();
        }
    }
}
