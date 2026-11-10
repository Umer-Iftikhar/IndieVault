using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Repositories.Implementations
{
    public class ScreenshotRepository : Repository<Screenshot>, IScreenshotRepository
    {
        public ScreenshotRepository(AppDbContext context) : base(context)
        {
        }
        public async Task AddScreenshotsAsync(List<Screenshot> screenshots)
        {
            await _context.Screenshots.AddRangeAsync(screenshots);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteScreenshotsByGameIdAsync(int gameId)
        {
            var oldScreenshots = await _context.Screenshots.Where(s => s.GameId == gameId).ToListAsync();
            _context.Screenshots.RemoveRange(oldScreenshots);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Screenshot>> GetScreenshotsByGameIdAsync(int gameId)
        {
            return await _context.Screenshots.Where(s => s.GameId == gameId).ToListAsync();
        }

    }
}
