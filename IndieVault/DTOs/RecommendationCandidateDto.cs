namespace IndieVault.DTOs
{
    // A game the player has not interacted with yet and could be recommended.
    public class RecommendationCandidateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImagePath { get; set; } = string.Empty;
        public string Developer { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public double AverageRating { get; set; }
    }
}
