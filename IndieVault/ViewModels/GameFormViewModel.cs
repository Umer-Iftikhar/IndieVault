using IndieVault.DTOs;

namespace IndieVault.ViewModels
{
    public class GameFormViewModel
    {
        public List<LookupDto> Genres { get; set; } = new();
        public List<LookupDto> Engines { get; set; } = new();
        public List<LookupDto> Platforms { get; set; } = new();
        public List<LookupDto> Tags { get; set; } = new();
    }
}
