using IndieVault.DTOs;
using IndieVault.Models;
using System.ComponentModel.DataAnnotations;

namespace IndieVault.ViewModels
{
    public class GameEditViewModel : GameFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        
        public IFormFile? CoverImage { get; set; } = null;

        public string ExistingCoverImagePath {  get; set; } = string.Empty; 

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

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
