using IndieVault.Repositories.Interfaces;
using IndieVault.Services.Implementations;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndieVault.Tests.TestHelpers
{
    public class AdminServiceBuilder
    {
        public Mock<IReviewRepository> Reviews { get; } = new();
        public Mock<IGameRepository> Games { get; } = new();
        public Mock<IGenreRepository> Genres { get; } = new();
        public Mock<IAdminStatisticsRepository> Statistics { get; } = new();

        public AdminService Build() => new(Reviews.Object, Games.Object, Genres.Object, Statistics.Object,
            NullLogger<AdminService>.Instance);
    }
}
