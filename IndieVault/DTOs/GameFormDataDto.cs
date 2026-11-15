namespace IndieVault.DTOs
{
    // This DTO is used to provide the necessary data for populating dropdowns and selection lists in the game upload and edit forms.
    public class GameFormDataDto
    {
        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
        public List<LookupDto> Engines { get; set; } = new List<LookupDto>();
        public List<LookupDto> Platforms { get; set; } = new List<LookupDto>();
        public List<LookupDto> Tags { get; set; } = new List<LookupDto>();
    }
}
