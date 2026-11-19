namespace IndieVault.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalGames { get; set; }
        public int TotalReviews { get; set; }
        public MostWishlistedGameDto? MostWishlistedGame { get; set; } = null;
        public List<UserByRoleDto> UsersByRole { get; set; } = new();
        public List<LookupDto> Genres { get; set; } = new();
        public List<AdminGameDto> Games { get; set; } = new();
    }
}
