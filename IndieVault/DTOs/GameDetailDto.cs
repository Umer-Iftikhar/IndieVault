
namespace IndieVault.DTOs
{
    public class GameDetailDto
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DeveloperName { get; set; } = string.Empty;
        public string DeveloperId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public string EngineName { get; set; } = string.Empty;
        public List<string> PlatformNames { get; set; } = new List<string>();
        public string CoverImagePath { get; set; } = string.Empty;
        public string DownloadLink { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public List<ScreenshotDto> Screenshots { get; set; } = new List<ScreenshotDto>();
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public bool HasWishlisted { get; set; } = false;
        public bool HasReviewed { get; set; } = false;
    }
}
