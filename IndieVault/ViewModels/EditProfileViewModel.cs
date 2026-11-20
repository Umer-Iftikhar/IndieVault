using System.ComponentModel.DataAnnotations;

namespace IndieVault.ViewModels
{
    public class EditProfileViewModel
    {
        [StringLength(100)]
        public string GithubUserName { get; set; } = string.Empty;
    }
}
