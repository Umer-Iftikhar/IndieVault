using IndieVault.Services.Interfaces;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndieVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GenresController : Controller
    {
        private readonly IAdminService _adminService;

        public GenresController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new GenreViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GenreViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _adminService.AdminCreateGenreAsync(model.GenreName, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the genre: {ex.Message}");
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int genreId, CancellationToken cancellationToken)
        {
            var genre = await _adminService.GetGenreByIdAsync(genreId, cancellationToken);
            var model = new GenreViewModel
            {
                GenreId = genre.Id,
                GenreName = genre.Name
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(GenreViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _adminService.AdminDeleteGenreAsync(model.GenreId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                model.Message = ex.Message;
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
