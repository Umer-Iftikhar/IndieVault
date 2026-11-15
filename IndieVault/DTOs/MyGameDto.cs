namespace IndieVault.DTOs
{
    public class MyGameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImagePath { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
    }
}
