using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using IndieVault.Tests.TestHelpers;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace IndieVault.Tests.Controllers
{
    public class AccountControllerProfileTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager = IdentityMocks.UserManager();
        private readonly Mock<IGitHubService> _gitHub = new();
        private readonly Mock<IGameService> _games = new();
        private readonly AccountController _controller;

        public AccountControllerProfileTests()
        {
            var signIn = IdentityMocks.SignInManager(_userManager.Object);
            _controller = new AccountController(_userManager.Object, signIn.Object, _gitHub.Object, _games.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("dev-1", "devone", "GameDev"))
            };
        }

        [Fact]
        public async Task Profile_UnknownUser_ReturnsNotFound()
        {
            _userManager.Setup(u => u.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

            Assert.IsType<NotFoundResult>(await _controller.Profile("missing"));
        }

        [Fact]
        public async Task Profile_BuildsViewModelWithGamesCountAndGitHubStats()
        {
            var joined = new DateTime(2026, 4, 1);
            var github = new GitHubProfileDto { PublicRepos = 6, TotalStars = 25 };
            _userManager.Setup(u => u.FindByIdAsync("dev-1")).ReturnsAsync(new ApplicationUser
            {
                Id = "dev-1", UserName = "devone", Email = "dev@indievault.test", GithubUserName = "umer", CreatedDate = joined
            });
            _gitHub.Setup(g => g.GetProfileAsync("umer")).ReturnsAsync(github);
            _games.Setup(g => g.GetDevGameCountAsync("dev-1")).ReturnsAsync(3);

            var result = await _controller.Profile("dev-1");

            var model = Assert.IsType<DevProfileViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("dev-1", model.UserId);
            Assert.Equal("devone", model.Name);
            Assert.Equal("dev@indievault.test", model.Email);
            Assert.Equal(joined, model.JoinDate);
            Assert.Equal(3, model.TotalGames);
            Assert.Same(github, model.GitHubProfile);
        }

        [Fact]
        public async Task EditProfile_Get_PrefillsCurrentGitHubUserName()
        {
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser { Id = "dev-1", GithubUserName = "umer" });

            var result = await _controller.EditProfile();

            var model = Assert.IsType<EditProfileViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("umer", model.GithubUserName);
        }

        [Fact]
        public async Task EditProfile_Get_UserMissing_ReturnsNotFound()
        {
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);

            Assert.IsType<NotFoundResult>(await _controller.EditProfile());
        }

        [Fact]
        public async Task EditProfile_Post_Success_SavesAndRedirectsToProfile()
        {
            var user = new ApplicationUser { Id = "dev-1", GithubUserName = "old" };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.EditProfile(new EditProfileViewModel { GithubUserName = "Umer-Iftikhar" });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Profile", redirect.ActionName);
            Assert.Equal("dev-1", redirect.RouteValues!["userId"]);
            Assert.Equal("Umer-Iftikhar", user.GithubUserName);
        }

        [Fact]
        public async Task EditProfile_Post_UpdateFails_ShowsErrors()
        {
            var user = new ApplicationUser { Id = "dev-1" };
            _userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _userManager.Setup(u => u.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Concurrency failure." }));

            var result = await _controller.EditProfile(new EditProfileViewModel { GithubUserName = "x" });

            Assert.IsType<ViewResult>(result);
            Assert.Contains(_controller.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == "Concurrency failure.");
        }

        [Fact]
        public async Task EditProfile_Post_InvalidModel_DoesNotUpdate()
        {
            _controller.ModelState.AddModelError("GithubUserName", "Too long");

            var result = await _controller.EditProfile(new EditProfileViewModel { GithubUserName = new string('x', 101) });

            Assert.IsType<ViewResult>(result);
            _userManager.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}
