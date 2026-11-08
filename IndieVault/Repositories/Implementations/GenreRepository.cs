using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndieVault.Repositories.Implementations
{
    public class GenreRepository : Repository<Genre>, IGenreRepository 
    {
        public GenreRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<bool> GenreExistsByNameAsync(string name)
        {
            return await _context.Genres.AnyAsync(g => g.Name.ToLower() == name.ToLower());
        }
        
        public async Task<Genre?> GetGenreByNameAsync(string name)
        {
            return await _context.Genres.FirstOrDefaultAsync(g => g.Name == name);
        }
    }
}
