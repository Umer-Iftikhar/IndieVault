
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndieVault.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            AdminDashboardDto DashboardDto = await _adminService.GetAdminDashboardDataAsync(); // Replace with actual method to get dashboard data

            var adminDashboardViewModel = new AdminDashboardViewModel // Map the DTO to the ViewModel
            {
                AdminName = User.Identity?.Name ?? "Admin",
                TotalGames = DashboardDto.TotalGames,
                TotalReviews = DashboardDto.TotalReviews,
                MostWishlistedGames = DashboardDto.MostWishlistedGame!,
                UsersByRole = DashboardDto.UsersByRole,
                Genres = DashboardDto.Genres,
                Games = DashboardDto.Games
            };

            return View(adminDashboardViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFeature(int gameId)
        {
            await _adminService.IsGameFeatureAsync(gameId);

            return RedirectToAction(nameof(Dashboard));
        }   
    }
}
