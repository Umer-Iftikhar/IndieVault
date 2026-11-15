using IndieVault.Models;

namespace IndieVault.Repositories.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task UpdateAsync(T entity);
        Task<T> CreateAsync(T entity);
        Task DeleteAsync(int id);
        Task<List<T>> GetAllAsync();
    }
}
