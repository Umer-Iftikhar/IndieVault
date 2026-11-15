namespace IndieVault.DTOs
{
    // A simple DTO for lookup data like genres, engines, platforms, and tags.
    public class LookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
