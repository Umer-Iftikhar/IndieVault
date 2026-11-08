using IndieVault.Models;

namespace IndieVault.Repositories.Interfaces
{
    public interface IEngineRepository : IRepository<Engine>
    {
        Task<Engine?> GetEngineByNameAsync(string name);
    }
}
