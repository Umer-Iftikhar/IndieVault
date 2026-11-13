

using System.ComponentModel.DataAnnotations;

namespace IndieVault.DTOs
{
    public class GameUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? CoverImage { get; set; } = null;
        public decimal Price { get; set; }
        public List<IFormFile>? Screenshots { get; set; }
        public int SelectedGenreId { get; set; }
        public int SelectedEngineId { get; set; }
        public List<string> SelectedPlatforms { get; set; } = new();
        public string DownloadLink { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public List<string> SelectedTags { get; set; } = new();
    }
}
