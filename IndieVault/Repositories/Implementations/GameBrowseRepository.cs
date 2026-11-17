using Dapper;
using IndieVault.DTOs;
using IndieVault.Enums;
using IndieVault.Repositories.Interfaces;
using MySqlConnector;

namespace IndieVault.Repositories.Implementations
{
    public class GameBrowseRepository : IGameBrowseRepository
    {
        private readonly IConfiguration _configuration;
        public GameBrowseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private MySqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            return new MySqlConnection(connectionString);
        }
        public async Task<IEnumerable<FeaturedGameDto>> GetFeaturedGamesAsync()
        {
            using var connection = CreateConnection();

            // SQL query to fetch featured games with their average ratings and developer names
            var sql = @"
                        select g.Title, g.CoverImagePath, gen.Name as GenreName, g.Price, COALESCE(AVG(re.Rating), 0) AS AverageRating, g.Id,
                        (
	                        select u.UserName
                            from  aspnetusers u
                            where u.Id = g.DeveloperId
                        ) AS Developer
                        from Games g
                        left Join genres gen ON g.GenreId = gen.Id
                        Left Join reviews re  ON g.Id = re.GameId 
                        where g.IsFeatured = true
                        group by g.Id, g.Title, g.CoverImagePath, gen.Name , g.Price, g.DeveloperId;";

            // Dapper will automatically map the results to the FeaturedGameDto
            var result = await connection.QueryAsync<FeaturedGameDto>(sql);

            return result;
        }
        public async Task<(List<GameBrowseDto> Games, int TotalCount)> GetPagedGamesAsync(int pageNumber, int pageSize, string? searchTerm,
            decimal? minPrice, decimal? maxPrice, int? genreId, List<int>? platformIds, SortBy sortBy)
        {
            using var connection = CreateConnection();

            // Dapper handles List<int> automatically for IN clauses
            var platformParam = (platformIds == null || !platformIds.Any()) ? null : platformIds;
            var platformFilter = (platformParam == null)
                ? "1=1"  // always true, no filter
                : @"EXISTS (
                    SELECT 1 FROM gameplatforms gp 
                    WHERE gp.GameId = g.Id AND gp.PlatformId IN @PlatformIds
                    )";

            // Parameters for Dapper
            var parameters = new DynamicParameters();
            parameters.Add("GenreId", genreId);
            parameters.Add("MinPrice", minPrice);
            parameters.Add("MaxPrice", maxPrice);
            parameters.Add("SearchTerm", searchTerm);
            parameters.Add("PlatformIds", platformParam);
            parameters.Add("SortBy", sortBy.ToString());
            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", (pageNumber - 1) * pageSize);

            // Combined SQL string
            var sql = $@"
                 -- 1. Get the Data
                SELECT g.Title, g.Price, g.ReleaseDate, gen.Name AS GenreName, 
                    COALESCE(AVG(re.Rating), 0) AS AverageRatings, g.Id, g.CoverImagePath,
                (
                    SELECT u.UserName 
                    FROM aspnetusers u 
                    WHERE u.Id = g.DeveloperId
                ) 
                AS Developer
                FROM games g 
                LEFT JOIN genres gen ON g.GenreId = gen.Id
                LEFT JOIN reviews re ON g.Id = re.GameId
                WHERE (@GenreId IS NULL OR g.GenreId = @GenreId)
                AND (@MinPrice IS NULL OR g.Price >= @MinPrice)
                AND (@MaxPrice IS NULL OR g.Price <= @MaxPrice)
                AND (@SearchTerm IS NULL OR g.Title LIKE CONCAT('%', @SearchTerm, '%'))
                AND ({platformFilter})
                GROUP BY g.Title, g.Price, g.ReleaseDate, gen.Name, g.DeveloperId, g.Id, g.CoverImagePath 
                ORDER BY 
                    CASE WHEN @SortBy = 'Rating' THEN COALESCE(AVG(re.Rating), 0) END DESC,
                    CASE WHEN @SortBy = 'PriceDesc' THEN g.Price END DESC,
                    CASE WHEN @SortBy = 'PriceAsc' THEN g.Price END ASC,
                    CASE WHEN @SortBy = 'Newest' THEN g.ReleaseDate END DESC,
                    g.Title ASC
                LIMIT @PageSize OFFSET @Offset;

                -- 2. Get the Count 
                SELECT COUNT(DISTINCT g.Id) 
                FROM games g 
                WHERE (@GenreId IS NULL OR g.GenreId = @GenreId)
                AND (@MinPrice IS NULL OR g.Price >= @MinPrice)
                AND (@MaxPrice IS NULL OR g.Price <= @MaxPrice)
                AND (@SearchTerm IS NULL OR g.Title LIKE CONCAT('%', @SearchTerm, '%'))
                AND ({platformFilter});";

            // Execute the combined SQL and read results
            using (var multi = await connection.QueryMultipleAsync(sql, parameters))
            {
                // Read the list of games
                var games = (await multi.ReadAsync<GameBrowseDto>()).ToList(); 

                var totalCount = await multi.ReadFirstAsync<int>();

                return (games, totalCount); // Return both the list of games and the total count
            }
        }
        public async Task<List<LookupDto>> GetGenreLookupsAsync()
        {
            // Simple query to fetch genre lookups
            using var connection = CreateConnection();

            var sql = "SELECT Id, Name FROM genres ORDER BY Name;";

            // Dapper will automatically map the results to the LookupDto
            var result = await connection.QueryAsync<LookupDto>(sql);

            return result.ToList();
        }
        public async Task<List<LookupDto>> GetPlatformLookupsAsync()
        {
            // Simple query to fetch platform lookups
            using var connection = CreateConnection();

            var sql = "SELECT Id, Name FROM platforms ORDER BY Name;";

            // Dapper will automatically map the results to the LookupDto
            var result = await connection.QueryAsync<LookupDto>(sql);
            
            return result.ToList();
        }
    }
}
