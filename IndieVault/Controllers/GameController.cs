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
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _gameService.DeleteGameAsync(id, currentUserId!);
            return RedirectToAction(nameof(MyGames));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var editGame = await _gameService.GetGameForEditAsync(id, currentUserId!);

            var viewModel = new GameEditViewModel
            {
                Id = id,
                CoverImage = null, // We don't want to populate the IFormFile, it will be uploaded if the user chooses a new one
                Title = editGame.Title,
                Description = editGame.Description,
                DownloadLink = editGame.DownloadLink,
                ExistingCoverImagePath = editGame.CoverImagePath,
                Price = editGame.Price,
                ReleaseDate = editGame.ReleaseDate,
                SelectedGenreId = editGame.SelectedGenreId,
                SelectedEngineId = editGame.SelectedEngineId,
                SelectedPlatforms = editGame.SelectedPlatformIds.Select(p => p.ToString()).ToList(),
                SelectedTags = editGame.SelectedTagIds.Select(t => t.ToString()).ToList(), // View model expects these fields to be strings, not ints.
            };
            await PopulateFormData(viewModel); // replaces the four dropdown lines

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameEditViewModel model)
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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var updateDto = new GameUpdateDto
            {
                Id = model.Id,
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
            await _gameService.UpdateGameAsync(updateDto, currentUserId!);

            return RedirectToAction(nameof(MyGames));
        }
    }
}
