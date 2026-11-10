using IndieVault.Models;

namespace IndieVault.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<bool> ReviewExistsAsync(string userId, int gameId);
        Task<int> CountReviewsAsync();
    }
}
