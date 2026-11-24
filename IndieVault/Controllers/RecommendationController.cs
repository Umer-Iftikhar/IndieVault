using IndieVault.Extensions;
using IndieVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace IndieVault.Controllers
{
    [Authorize(Roles = "Player")]
    public class RecommendationController : Controller
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        // Served from cache most of the time, so it is not rate limited
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recommendations = await _recommendationService.GetRecommendationsAsync(userId!, cancellationToken);
            return View(recommendations);
        }

        // Every refresh costs one AI call, so this is the action that is rate limited per user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.AiRecommendations)]
        public IActionResult Refresh()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _recommendationService.ClearRecommendations(userId!);
            return RedirectToAction(nameof(Index));
        }
    }
}
