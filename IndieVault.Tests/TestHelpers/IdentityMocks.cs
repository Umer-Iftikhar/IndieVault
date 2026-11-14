using System.Security.Claims;
using IndieVault.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.TestHelpers
{
    public static class IdentityMocks
    {
        public static Mock<UserManager<ApplicationUser>> UserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        public static Mock<SignInManager<ApplicationUser>> SignInManager(UserManager<ApplicationUser> userManager)
        {
            return new Mock<SignInManager<ApplicationUser>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!);
        }

        // ClaimsPrincipal for a signed-in user, or an anonymous one when userId is null.
        public static ClaimsPrincipal User(string? userId = null, string? userName = null, params string[] roles)
        {
            if (userId == null)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName ?? userId)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        public static ControllerContext ControllerContextFor(ClaimsPrincipal user)
        {
            return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }
    }
}
