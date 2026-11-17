namespace IndieVault.DTOs
{
    public class FeaturedGameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Developer { get; set; } = string.Empty;
        public double AverageRating { get; set; }
    }
}
