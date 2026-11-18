namespace IndieVault.DTOs
{
    public class WishlistItemDto
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImagePath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public  string GameGenre { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
