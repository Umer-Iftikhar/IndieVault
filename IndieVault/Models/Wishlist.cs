
using System.ComponentModel.DataAnnotations.Schema;

namespace IndieVault.Models
{
    public class Wishlist : BaseEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int GameId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public Game Game { get; set; } = null!;
    }
}
