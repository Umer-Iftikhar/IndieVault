using System.Security.Claims;
using IndieVault.Controllers;
using IndieVault.DTOs;
using IndieVault.Extensions;
using IndieVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moq;

namespace IndieVault.Tests.Controllers
{
    public class RecommendationControllerTests
    {
        private readonly Mock<IRecommendationService> _service = new();
        private readonly RecommendationController _controller;

        public RecommendationControllerTests()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "player-1"),
                new Claim(ClaimTypes.Name, "playerone"),
                new Claim(ClaimTypes.Role, "Player")
            }, "TestAuth");

            _controller = new RecommendationController(_service.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
                }
            };
        }

        [Fact]
        public async Task Index_ShowsRecommendationsForCurrentPlayer()
        {
            var recommendations = new RecommendationResultDto
            {
                GeneratedByAi = true,
                Games = new List<RecommendedGameDto> { new() { Reason = "You will like it" } }
            };
            _service.Setup(s => s.GetRecommendationsAsync("player-1", It.IsAny<CancellationToken>())).ReturnsAsync(recommendations);

            var result = await _controller.Index(CancellationToken.None);

            Assert.Same(recommendations, Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task Index_PassesTheRequestCancellationTokenToTheService()
        {
            using var cts = new CancellationTokenSource();
            _service.Setup(s => s.GetRecommendationsAsync("player-1", cts.Token)).ReturnsAsync(new RecommendationResultDto());

            await _controller.Index(cts.Token);

            _service.Verify(s => s.GetRecommendationsAsync("player-1", cts.Token), Times.Once);
        }

        [Fact]
        public void Refresh_ClearsCachedRecommendationsAndRedirectsToIndex()
        {
            var result = _controller.Refresh();

            Assert.Equal(nameof(RecommendationController.Index), Assert.IsType<RedirectToActionResult>(result).ActionName);
            _service.Verify(s => s.ClearRecommendations("player-1"), Times.Once);
        }

        [Fact]
        public void Controller_IsRestrictedToPlayers()
        {
            var auth = typeof(RecommendationController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single();

            Assert.Equal("Player", auth.Roles);
        }

        [Fact]
        public void Refresh_IsRateLimitedAndProtectedAgainstForgery()
        {
            var refresh = typeof(RecommendationController).GetMethod(nameof(RecommendationController.Refresh))!;

            var rateLimit = refresh.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true)
                .Cast<EnableRateLimitingAttribute>().Single();
            Assert.Equal(RateLimitPolicies.AiRecommendations, rateLimit.PolicyName);
            Assert.NotEmpty(refresh.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true));
            Assert.NotEmpty(refresh.GetCustomAttributes(typeof(HttpPostAttribute), true));
        }

        [Fact]
        public void Index_IsNotRateLimited()
        {
            // The page is served from cache, so only Refresh needs the limit
            var index = typeof(RecommendationController).GetMethod(nameof(RecommendationController.Index))!;

            Assert.Empty(index.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true));
            Assert.Empty(typeof(RecommendationController).GetCustomAttributes(typeof(EnableRateLimitingAttribute), true));
        }
    }
}
