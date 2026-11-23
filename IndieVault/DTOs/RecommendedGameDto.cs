namespace IndieVault.DTOs
{
    public class RecommendedGameDto
    {
        // Reuses the browse DTO so the page can render the existing _GameCard partial
        public GameBrowseDto Game { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }
}
