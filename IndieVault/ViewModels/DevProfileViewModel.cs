using IndieVault.DTOs;
namespace IndieVault.ViewModels
{
    public class DevProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int TotalGames { get; set; }
        public DateTime JoinDate { get; set; }
        public GitHubProfileDto? GitHubProfile { get; set; }
    }
}
