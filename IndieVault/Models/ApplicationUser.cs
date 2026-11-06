using System.ComponentModel.DataAnnotations;

namespace IndieVault.Models
{
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        [StringLength(100)]
        public string GithubUserName { get; set; } = string.Empty;  
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
