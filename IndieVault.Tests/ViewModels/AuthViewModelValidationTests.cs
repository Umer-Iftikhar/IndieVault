using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;

namespace IndieVault.Tests.ViewModels
{
    public class AuthViewModelValidationTests
    {
        private static RegisterViewModel ValidRegistration() => new()
        {
            UserName = "umer",
            Email = "umer@indievault.test",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        };

        [Fact]
        public void Register_ValidModel_PassesValidation()
        {
            Assert.Empty(ModelValidator.Validate(ValidRegistration()));
        }

        [Fact]
        public void Register_PasswordsDoNotMatch_FailsOnConfirmPassword()
        {
            var model = ValidRegistration();
            model.ConfirmPassword = "Different1!";

            var results = ModelValidator.Validate(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
        }

        [Fact]
        public void Register_UserNameTooShort_Fails()
        {
            var model = ValidRegistration();
            model.UserName = "u";

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(RegisterViewModel.UserName)));
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("")]
        public void Register_InvalidEmail_Fails(string email)
        {
            var model = ValidRegistration();
            model.Email = email;

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(RegisterViewModel.Email)));
        }

        [Fact]
        public void Register_PasswordUnder6Chars_Fails()
        {
            var model = ValidRegistration();
            model.Password = "Ab1!";
            model.ConfirmPassword = "Ab1!";

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(RegisterViewModel.Password)));
        }

        [Fact]
        public void Login_ValidModel_PassesValidation()
        {
            var model = new LoginViewModel { Email = "umer@indievault.test", Password = "Passw0rd!" };

            Assert.Empty(ModelValidator.Validate(model));
        }

        [Fact]
        public void Login_MissingFields_FailsOnEmailAndPassword()
        {
            var results = ModelValidator.Validate(new LoginViewModel());

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Email)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Password)));
        }

        [Fact]
        public void Login_InvalidEmail_Fails()
        {
            var model = new LoginViewModel { Email = "umer", Password = "Passw0rd!" };

            Assert.Contains(ModelValidator.Validate(model), r => r.MemberNames.Contains(nameof(LoginViewModel.Email)));
        }
    }
}
