using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace IndieVault.Repositories.Implementations
{
    public class EngineRepository : Repository<Engine>, IEngineRepository
    {
        public EngineRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Engine?> GetEngineByNameAsync(string name)
        {
            return await _context.Engines.FirstOrDefaultAsync(e => e.Name == name);
        }
    }
}
