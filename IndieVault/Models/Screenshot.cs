using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndieVault.Models
{
    public class Screenshot : BaseEntity
    {
        public string ImagePath { get; set; } = String.Empty;
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
    }
}
