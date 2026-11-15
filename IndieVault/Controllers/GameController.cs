using IndieVault.DTOs;
using IndieVault.Models;
using IndieVault.Services.Interfaces;
using IndieVault.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndieVault.Controllers
{
    [Authorize(Roles = "GameDev")]
    public class GameController : Controller
    {
        private readonly IGameService _gameService;
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        private async Task PopulateFormData(GameFormViewModel model)
        {
            var formData = await _gameService.GetFormDataAsync();
            model.Genres = formData.Genres;
            model.Engines = formData.Engines;
            model.Platforms = formData.Platforms;
            model.Tags = formData.Tags;
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var model = new GameUploadViewModel();
            await PopulateFormData(model);
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(GameUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateFormData(model);
                return View(model);
            }

            // Validation: Max 5 screenshots
            if (model.Screenshots != null && model.Screenshots.Count > 5)
            {
                ModelState.AddModelError("Screenshots", "Maximum 5 screenshots allowed.");
                await PopulateFormData(model);
                return View(model);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var uploadDto = new GameUploadDto
            {
                Title = model.Title,
                Description = model.Description,
                CoverImage = model.CoverImage,
                Price = model.Price,
                Screenshots = model.Screenshots,
                SelectedGenreId = model.SelectedGenreId,
                SelectedEngineId = model.SelectedEngineId,
                DownloadLink = model.DownloadLink,
                ReleaseDate = model.ReleaseDate,
                SelectedPlatforms = model.SelectedPlatforms,
                SelectedTags = model.SelectedTags
            };

            await _gameService.UploadGameAsync(uploadDto, currentUserId);

            return RedirectToAction(nameof(MyGames));
        }


        [HttpGet]
        public async Task<IActionResult> MyGames()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var games = await _gameService.GetMyGamesAsync(currentUserId!);

            return View(games);
        }
    }
}
