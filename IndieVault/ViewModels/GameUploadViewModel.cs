using IndieVault.DTOs;
using System.ComponentModel.DataAnnotations;


namespace IndieVault.ViewModels
{
    public class GameUploadViewModel : GameFormViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Upload)]
        public IFormFile CoverImage { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [DataType(DataType.Upload)]
        public List<IFormFile>? Screenshots { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a genre.")]
        public int SelectedGenreId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an engine.")]
        public int SelectedEngineId { get; set; }

        public List<string> SelectedPlatforms { get; set; } = new();

        [Required]
        public string DownloadLink { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; } 

        public List<string> SelectedTags { get; set; } = new();
    }
}
