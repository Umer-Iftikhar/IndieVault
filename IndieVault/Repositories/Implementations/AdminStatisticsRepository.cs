using Dapper;
using IndieVault.DTOs;
using IndieVault.Repositories.Interfaces;
using MySqlConnector;

namespace IndieVault.Repositories.Implementations
{
    public class AdminStatisticsRepository : IAdminStatisticsRepository
    {
        private readonly IConfiguration _configuration;
        public AdminStatisticsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private MySqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            return new MySqlConnection(connectionString);
        }
        public async Task<MostWishlistedGameDto?> MostWishlistedGameAsync()
        {
            var sql = @"
                   SELECT g.Title, COUNT(*) as WishlistCount
                   FROM Wishlists w
                   INNER JOIN Games g ON w.GameId = g.Id
                   GROUP BY g.Title
                   ORDER BY WishlistCount DESC
                   LIMIT 1;";
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<MostWishlistedGameDto>(sql);
        }
        public async Task<List<UserByRoleDto>> GetUsersByRoleAsync()
        {
            var sql = @"
                SELECT 
                    r.Name AS RoleName, 
                    COUNT(ur.UserId) AS UserCount
                 FROM AspNetRoles r
                 INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
                 INNER JOIN AspNetUsers u ON ur.UserId = u.Id
                 GROUP BY r.Name;";
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<UserByRoleDto>(sql);
            return result.ToList();
        }
    }
}
