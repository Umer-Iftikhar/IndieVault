using IndieVault.Data;
using IndieVault.Models;
using IndieVault.Repositories.Interfaces;

namespace IndieVault.Repositories.Implementations
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }
    }
}
