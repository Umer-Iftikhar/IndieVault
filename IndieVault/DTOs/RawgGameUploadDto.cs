namespace IndieVault.DTOs
{
    public class RawgGameUploadDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string CoverImagePath { get; set; } = string.Empty;
        public string? DownloadLink { get; set; } = string.Empty;
        public int GenreId { get; set; }
        public int EngineId { get; set; }
        public string DeveloperId { get; set; } = string.Empty;
        public int ExternalApiId { get; set; }
        public string ExternalApiSource { get; set; } = "RAWG";
        public bool IsFromExternalApi { get; set; } = true;
    }
}
