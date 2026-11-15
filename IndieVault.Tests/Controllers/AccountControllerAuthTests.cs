using IndieVault.Controllers;
using IndieVault.Models;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IndieVault.Tests.Controllers
{
    public class AccountControllerAuthTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        private readonly AccountController _controller;

        public AccountControllerAuthTests()
        {
            _userManager = IdentityMocks.UserManager();
            _signInManager = IdentityMocks.SignInManager(_userManager.Object);
            _controller = new AccountController(_userManager.Object, _signInManager.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User())
            };
        }

        private static RegisterViewModel Registration() => new()
        {
            UserName = "newuser",
            Email = "new@indievault.test",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!"
        };

        private static void AssertRedirectsHome(IActionResult result)
        {
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task RegisterPlayer_Get_WhenSignedIn_RedirectsHome()
        {
            _signInManager.Setup(s => s.IsSignedIn(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(true);

            AssertRedirectsHome(await _controller.RegisterPlayer());
        }

        [Fact]
        public async Task RegisterPlayer_Get_WhenAnonymous_ShowsForm()
        {
            _signInManager.Setup(s => s.IsSignedIn(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(false);

            Assert.IsType<ViewResult>(await _controller.RegisterPlayer());
        }

        [Fact]
        public async Task RegisterPlayer_Post_InvalidModel_ReturnsFormWithoutCreatingUser()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var model = Registration();

            var result = await _controller.RegisterPlayer(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            _userManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterPlayer_Post_Success_AssignsPlayerRoleAndSignsIn()
        {
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), "Passw0rd!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.RegisterPlayer(Registration());

            AssertRedirectsHome(result);
            _userManager.Verify(u => u.AddToRoleAsync(
                It.Is<ApplicationUser>(a => a.UserName == "newuser" && a.Email == "new@indievault.test"), "Player"), Times.Once);
            _signInManager.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task RegisterGameDev_Post_Success_AssignsGameDevRole()
        {
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.RegisterGameDev(Registration());

            AssertRedirectsHome(result);
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "GameDev"), Times.Once);
            _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Player"), Times.Never);
        }

        [Fact]
        public async Task RegisterPlayer_Post_IdentityFailure_ShowsErrorsAndDoesNotSignIn()
        {
            _userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email is already taken." }));

            var result = await _controller.RegisterPlayer(Registration());

            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == "Email is already taken.");
            _signInManager.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task Login_Post_UnknownEmail_ShowsInvalidLoginAttempt()
        {
            _userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.Login(new LoginViewModel { Email = "nobody@indievault.test", Password = "Passw0rd!" });

            Assert.IsType<ViewResult>(result);
            Assert.Contains(_controller.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == "Invalid login attempt.");
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_SignsInWithLockoutAndRedirectsHome()
        {
            var user = new ApplicationUser { UserName = "umer", Email = "umer@indievault.test" };
            _userManager.Setup(u => u.FindByEmailAsync("umer@indievault.test")).ReturnsAsync(user);
            _signInManager.Setup(s => s.PasswordSignInAsync("umer", "Passw0rd!", false, true))
                .ReturnsAsync(IdentitySignInResult.Success);

            var result = await _controller.Login(new LoginViewModel { Email = "umer@indievault.test", Password = "Passw0rd!" });

            AssertRedirectsHome(result);
            _signInManager.Verify(s => s.PasswordSignInAsync("umer", "Passw0rd!", false, true), Times.Once);
        }

        [Fact]
        public async Task Login_Post_WrongPassword_ShowsInvalidLoginAttempt()
        {
            var user = new ApplicationUser { UserName = "umer", Email = "umer@indievault.test" };
            _userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _signInManager.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(IdentitySignInResult.Failed);

            var result = await _controller.Login(new LoginViewModel { Email = "umer@indievault.test", Password = "wrong" });

            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Logout_SignsOutAndRedirectsHome()
        {
            var result = await _controller.Logout();

            AssertRedirectsHome(result);
            _signInManager.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Fact]
        public void AccessDenied_ReturnsView()
        {
            Assert.IsType<ViewResult>(_controller.AccessDenied());
        }
    }
}
