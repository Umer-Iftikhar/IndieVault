using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;

namespace IndieVault.Tests.ViewModels
{
    public class EditProfileViewModelValidationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("Umer-Iftikhar")]
        public void GithubUserName_Within100Chars_Passes(string name)
        {
            Assert.Empty(ModelValidator.Validate(new EditProfileViewModel { GithubUserName = name }));
        }

        [Fact]
        public void GithubUserName_Over100Chars_Fails()
        {
            var results = ModelValidator.Validate(new EditProfileViewModel { GithubUserName = new string('x', 101) });

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(EditProfileViewModel.GithubUserName)));
        }
    }
}
