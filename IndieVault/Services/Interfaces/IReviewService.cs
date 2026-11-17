using IndieVault.DTOs;

namespace IndieVault.Services.Interfaces
{
    public interface IReviewService
    {
        Task GetReviewFormAsync(int id);
        Task CreateReviewAsync(CreateReviewDto createReviewDto, string userId);
        Task<bool> HasUserReviewedAsync(int gameId, string userId);
        Task DeleteReviewAsync(int reviewId, string userId, bool isAdmin);
    }
}
