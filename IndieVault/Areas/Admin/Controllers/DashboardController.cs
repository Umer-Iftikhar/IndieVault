using IndieVault.Extensions;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IndieVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IRawgApiService _rawgApiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IAdminService adminService, IRawgApiService rawgApiService, ILogger<DashboardController> logger)
        {
            _adminService = adminService;
            _rawgApiService = rawgApiService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var dashboardDto = await _adminService.GetAdminDashboardDataAsync(cancellationToken);

            var adminDashboardViewModel = new AdminDashboardViewModel // Map the DTO to the ViewModel
            {
                AdminName = User.Identity?.Name ?? "Admin",
                TotalGames = dashboardDto.TotalGames,
                TotalReviews = dashboardDto.TotalReviews,
                MostWishlistedGames = dashboardDto.MostWishlistedGame!,
                UsersByRole = dashboardDto.UsersByRole,
                Genres = dashboardDto.Genres,
                Games = dashboardDto.Games
            };

            return View(adminDashboardViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeature(int gameId, CancellationToken cancellationToken)
        {
            await _adminService.IsGameFeatureAsync(gameId, cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        // Each sync makes one RAWG call per game, so it is limited to once every 10 minutes
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.RawgSync)]
        public async Task<IActionResult> SyncGames(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _rawgApiService.SynchronizeGamesFromApiAsync(cancellationToken);

                TempData["Message"] = $"{result.Imported} games imported, {result.LinksUpdated} download links updated.";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                TempData["Error"] = "RAWG sync failed. Check logs.";
                _logger.LogError(ex, "RAWG sync failed");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
