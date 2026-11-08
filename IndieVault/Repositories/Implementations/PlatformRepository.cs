using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;

namespace IndieVault.Repositories.Implementations
{
    public class PlatformRepository : Repository<Platform>, IPlatformRepository
    {
        public PlatformRepository(AppDbContext context) : base(context)
        {
        }
    }
}
