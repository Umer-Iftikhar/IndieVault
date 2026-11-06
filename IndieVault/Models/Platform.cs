using System.ComponentModel.DataAnnotations;

namespace IndieVault.Models
{
    public class Platform : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public List<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    }
}
