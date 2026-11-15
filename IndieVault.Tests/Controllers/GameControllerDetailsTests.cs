using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class GameControllerDetailsTests
    {
        [Fact]
        public async Task Details_AnonymousVisitor_PassesEmptyUserId()
        {
            var service = new Mock<IGameService>();
            var dto = new GameDetailDto { GameId = 3 };
            service.Setup(s => s.GetGameDetailsAsync(3, string.Empty)).ReturnsAsync(dto);
            var controller = new GameController(service.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User())
            };

            var result = await controller.Details(3);

            Assert.Same(dto, Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task Details_SignedInUser_PassesTheirId()
        {
            var service = new Mock<IGameService>();
            service.Setup(s => s.GetGameDetailsAsync(3, "player-1")).ReturnsAsync(new GameDetailDto { GameId = 3 });
            var controller = new GameController(service.Object)
            {
                ControllerContext = IdentityMocks.ControllerContextFor(IdentityMocks.User("player-1", "playerone", "Player"))
            };

            await controller.Details(3);

            service.Verify(s => s.GetGameDetailsAsync(3, "player-1"), Times.Once);
        }

        [Fact]
        public void Details_IsPublic_WhileRestOfControllerRequiresGameDev()
        {
            var classAuth = typeof(GameController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single();
            var details = typeof(GameController).GetMethod(nameof(GameController.Details))!;

            Assert.Equal("GameDev", classAuth.Roles);
            Assert.NotEmpty(details.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        }
    }
}
