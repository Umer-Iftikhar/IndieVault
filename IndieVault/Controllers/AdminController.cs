
using IndieVault.DTOs;
using IndieVault.Services.Interfaces;
using IndieVault.Services.Interfaces.ExternalApis;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndieVault.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IRawgApiService _rawgApiService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, IRawgApiService rawgApiService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _rawgApiService = rawgApiService;
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

        [HttpGet]
        public async Task<IActionResult> CreateGenre()
        {
            var model = new GenreViewModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateGenre(GenreViewModel model)
        {
            
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                await _adminService.AdminCreateGenreAsync(model.GenreName);

            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the genre: {ex.Message}");
                return View(model);
            }

            return RedirectToAction(nameof(Dashboard));
        }
        [HttpGet]
        public async Task<IActionResult> GenreDelete(int genreId)
        {
            var genre = await _adminService.GetGenreByIdAsync(genreId);
            var model = new GenreViewModel
            {
                GenreId = genre.Id,
                GenreName = genre.Name
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> GenreDelete(GenreViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                await _adminService.AdminDeleteGenreAsync(model.GenreId);

            }
            catch (InvalidOperationException ex)
            {
                model.Message = ex.Message;
                return View(model);
            }

            return RedirectToAction(nameof(Dashboard));
            
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFeature(int gameId)
        {
            await _adminService.IsGameFeatureAsync(gameId);

            return RedirectToAction(nameof(Dashboard));
        }   
        [HttpPost]
        public async Task<IActionResult> SyncGames()
        {
            try
            {
                int syncedCount = await _rawgApiService.SynchronizeGamesFromApiAsync();

                TempData["Message"] = $"{syncedCount} games synced successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "RAWG sync failed. Check logs.";

                // optional but smart
                _logger.LogError(ex, "RAWG sync failed");
            }
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
