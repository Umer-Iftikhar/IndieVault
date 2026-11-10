using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Repositories.Implementations
{
    public class WishlistRepository : Repository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<bool> WishlistExistsAsync(string userId, int gameId)
        {
            return await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.GameId == gameId);
        }
        public async Task<List<Wishlist>> GetWishlistByUserIdAsync(string userId)
        {
            var wishlistGames = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Game)
                    .ThenInclude(g => g.Genre)
                .ToListAsync();
            return wishlistGames;
        }
        public async Task<Wishlist?> GetWishlistEntryAsync(string userId, int gameId)
        {
            return await _context.Wishlists
                .FirstOrDefaultAsync(w =>
                    w.UserId == userId &&
                    w.GameId == gameId);
        }
    }
}
