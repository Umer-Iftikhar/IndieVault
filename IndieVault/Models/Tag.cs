using System.ComponentModel.DataAnnotations;

namespace IndieVault.Models
{
    public class Tag :BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public List<GameTag> GameTags { get; set; } = new List<GameTag>();
    }
}
